using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer;
using NLog;

namespace Cinenic.Resources {
	
	public static class ResourceManager {

		public const string ASSETS_DIRECTORY_NAME = "assets";
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private static readonly Dictionary<string, object> _loadedResources = [];
		private static readonly Dictionary<string, FileSystemWatcher> _resourceWatchers = [];
		private static readonly ConcurrentDictionary<string, bool> _reloadLocks = [];

		static ResourceManager() {
			// dispose all remaining resources when process exits, primarily to save them
			AppDomain.CurrentDomain.ProcessExit += (_, _) => {
				foreach(var resourceObject in _loadedResources.Values) {
					((IDisposable) resourceObject).Dispose();
				}
			};

			AppDomain.CurrentDomain.UnhandledException += (_, _) => {
				foreach(var resourceObject in _loadedResources.Values) {
					((IDisposable) resourceObject).Dispose();
				}
			};
		}
		
		public static Ref<TResource>? Load<TResource>(
			IPlatform platform,
			string path,
			bool required = true,
			//bool explicitSubpath = false,
			bool explicitPath = false,
			Assembly? assembly = null
		)
			where TResource : class, IResource, new()
		{
			// if(Path.IsPathFullyQualified(path) && !absolutePath) {
			// 	throw new ArgumentException("Resource path must be relative to the asset directory root", path);
			// }

			assembly ??= Assembly.GetCallingAssembly();
			var @namespace = assembly.GetName().Name;

			if(@namespace is null) {
				_logger.Warn("Calling assembly has no name? Resource loading might break!");
				_logger.Warn("Setting namespace to \"unknown\"");
				@namespace = "unknown";
			}
			
			var baseDirectory = Path.GetDirectoryName(assembly.Location)!;
			
			//if(!explicitSubpath) {
				baseDirectory += Path.DirectorySeparatorChar + ASSETS_DIRECTORY_NAME;
				baseDirectory += Path.DirectorySeparatorChar + @namespace;
			//}
			
			var fullPath = explicitPath ? path : baseDirectory + Path.DirectorySeparatorChar + path;
			fullPath = Path.GetFullPath(fullPath); // resolve the real path
			
			if(_loadedResources.TryGetValue(fullPath, out var loadedResource)) {
				Debug.Assert(loadedResource is TResource);
				
				_logger.Debug("Returning already-loaded resource for {FilePath}", fullPath);
				return new((TResource) loadedResource);
			}
			
			if(!File.Exists(fullPath)) {
				_logger.Warn("Resource file not found: {FilePath}", fullPath);

				if(required) {
					throw new FileNotFoundException("Required resource file not found", fullPath);
				}

				return null;
			}
			
			var tempInstance = new TResource();
			var settingsType = tempInstance.SettingsType;
			var fileExtensions = tempInstance.FileExtensions;

			if(!fileExtensions.Any(e => fullPath.EndsWith(e, StringComparison.OrdinalIgnoreCase))) {
				_logger.Warn("Invalid extension for resource type {Type}", typeof(TResource).Name);
				_logger.Warn("Valid extensions are: {Extensions}", string.Join(", ", fileExtensions));

				if(required) {
					throw new ArgumentException($"Invalid extension for resource type {typeof(TResource).Name}", path);
				}

				return null;
			}

			ImportMetadata? importSettings = null;

			if(File.Exists(fullPath + ImportMetadata.FileExtension)) {
				using var importStream = new FileStream(fullPath + ImportMetadata.FileExtension, FileMode.Open);
				importSettings = (ImportMetadata?) JsonSerializer.Deserialize(importStream, settingsType, ImportMetadata.DefaultSerializerOptions);
			}

			if(importSettings is null) {
				_logger.Debug("Creating a new default ImportSettings instance as an existing one could not be found");
				importSettings = (ImportMetadata) settingsType.GetConstructor([]).Invoke(null);
			}

			var format = ResourceRegistry.GetFormat(importSettings.FormatId);

			if(format is null) {
				_logger.Warn("Could not find import format for resource type {Type}", importSettings.FormatId);

				if(required) {
					throw new TargetException($"Could not find import format for resource type {importSettings.FormatId}");
				}

				return null;
			}

			using var fileStream = new FileStream(fullPath, FileMode.Open);

			var instance = format.Value.Constructor.Invoke(null);

			try {
				format.Value.LoadMethod.Invoke(
					instance,
					[platform, fullPath, fileStream, assembly, importSettings]
				);
			} catch(Exception e) {
				_logger.Warn(e, "Resource {Path} could not be loaded", fullPath);

				if(required) {
                    throw;
                }

				return null;
			}
			
			Debug.Assert(instance is not null);
			Debug.Assert(instance is TResource);
			var resourceInstance = (TResource) instance;

			// TODO configurable hot-reload outside of debug builds?
		#if DEBUG
			if(!_resourceWatchers.TryGetValue(@namespace, out var watcher)) {
				watcher = new FileSystemWatcher(baseDirectory) {
					EnableRaisingEvents = true,
					NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
					IncludeSubdirectories = true
				};

				_resourceWatchers[@namespace] = watcher;
			}

			void WatcherChanged(object s, FileSystemEventArgs e) {
				// TODO confirm if dependency handling works
				if(
					(e.FullPath == fullPath || resourceInstance.Dependencies.Any(dependency => dependency.FilePath == e.FullPath))
					&& !_reloadLocks.GetValueOrDefault(fullPath, false)
				) {
					_reloadLocks[fullPath] = true;
					
					platform.PlatformThread.ScheduleAction(() => {
						resourceInstance.Reload();
						_reloadLocks[fullPath] = false;
					});
				}
			}

			watcher.Changed += WatcherChanged;
			watcher.Created += WatcherChanged;
			watcher.Renamed += WatcherChanged;
		#endif
			
			_loadedResources[fullPath] = resourceInstance;
			
			resourceInstance.Freed += _ => {
			#if DEBUG
				if(_resourceWatchers.TryGetValue(@namespace, out var watcher)) {
					watcher.Changed -= WatcherChanged;
					watcher.Created -= WatcherChanged;
					watcher.Renamed -= WatcherChanged;
				}
			#endif
				
				_loadedResources.Remove(fullPath);
				_logger.Trace("Removed resource {Path} from loaded resources as it's no longer valid", fullPath);
			};
			
			_logger.Debug("Successfully loaded resource of type {Type} at {Path}", typeof(TResource).Name, fullPath);
			return new(resourceInstance);
		}

		/*public static void AddFormatAssembly(Assembly assembly) {
			_formatAssemblies.Add(assembly);
			_logger.Debug("Assembly added for format loading: {AssemblyName}", assembly.GetName().FullName);
		}
		
		public static void Initialize() {
			foreach(var assembly in _formatAssemblies) {
				foreach(var type in assembly.GetTypes()) {
					if(!type.IsSubclassOf(typeof(IResource))) continue;
					
					var initInstance = type.GetConstructor([])?.Invoke(null);
		
					if(initInstance is null) {
						_logger.Warn("Could not create instance of type {TypeName}, skipping", type.FullName ?? type.Name);
						continue;
					}
		
					// these should never be null if inheriting Resource
					var formatIdProperty = type.GetProperty("TypeId")!;
					var formatId = (string) formatIdProperty.GetValue(initInstance)!;
					
					_formats[formatId] = type;
					
					_logger.Info(
						"Loaded resource format {FormatId} ({TypeName}) from {AssemblyName}",
						formatId, type.Name, assembly.FullName ?? "?"
					);
				}
			}
		}*/
	}
}
