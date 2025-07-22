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

			var metaPath = fullPath + ImportMetadata.FILE_EXTENSION;
			
			if(_loadedResources.TryGetValue(fullPath, out var loadedResource)) {
				Debug.Assert(loadedResource is TResource);
				
				_logger.Debug("Returning already-loaded resource for {FilePath}", fullPath);
				return new((TResource) loadedResource);
			}
			
			if(!File.Exists(metaPath) && !File.Exists(fullPath)) {
				_logger.Warn("Resource file not found: {FilePath}", fullPath);

				if(required) {
					throw new FileNotFoundException("Required resource file not found", fullPath);
				}

				return null;
			}
			
			var tempInstance = new TResource();
			var settingsType = tempInstance.SettingsType;
			var fileExtensions = tempInstance.FileExtensions;
			
			ImportMetadata? importMeta = null;

			if(File.Exists(metaPath)) {
				using var importStream = new FileStream(metaPath, FileMode.Open);
				importMeta = (ImportMetadata?) JsonSerializer.Deserialize(importStream, settingsType, ImportMetadata.DefaultSerializerOptions);
			}
			
			if(importMeta is null) {
				_logger.Debug("Creating a new default ImportSettings instance as an existing one could not be found");
				importMeta = (ImportMetadata) settingsType.GetConstructor([]).Invoke(null);
			}

			importMeta.MetaPath = metaPath;
			
			string? realPath = fullPath;
			Stream stream;

			if(importMeta.File is not null) {
				if(importMeta.File.StartsWith('/')) {
					realPath = baseDirectory + Path.DirectorySeparatorChar + importMeta.File;
					realPath = Path.GetFullPath(realPath);
				} else {
					realPath = Path.GetDirectoryName(metaPath) + Path.DirectorySeparatorChar + importMeta.File;
					realPath = Path.GetFullPath(realPath);
				}
			}

			if(importMeta.Data is not null) {
				_logger.Debug("Loading resource from embedded data; reloading will be disabled");
				
				realPath = null;
				stream = new MemoryStream(importMeta.Data);
			} else {
				if(!fileExtensions.Any(e => realPath.EndsWith(e, StringComparison.OrdinalIgnoreCase))) {
					_logger.Warn("Invalid extension for resource type {Type}", typeof(TResource).Name);
					_logger.Warn("Valid extensions are: {Extensions}", string.Join(", ", fileExtensions));

					if(required) {
						throw new ArgumentException($"Invalid extension for resource type {typeof(TResource).Name}", path);
					}

					return null;
				}
				
				stream = new FileStream(realPath, FileMode.Open);
			}

			var format = ResourceRegistry.GetFormat(importMeta.FormatId);

			if(format is null) {
				stream.Dispose();
				_logger.Warn("Could not find import format for resource type {Type}", importMeta.FormatId);

				if(required) {
					throw new TargetException($"Could not find import format for resource type {importMeta.FormatId}");
				}

				return null;
			}

			var instance = format.Value.Constructor.Invoke(null);

			try {
				format.Value.LoadMethod.Invoke(
					instance,
					[platform, realPath, stream, assembly, importMeta]
				);
			} catch(Exception e) {
				_logger.Warn(e, "Resource {Path} could not be loaded", realPath);

				if(required) {
					throw;
				}

				return null;
			} finally {
				stream.Dispose();
			}
			
			Debug.Assert(instance is not null);
			Debug.Assert(instance is TResource);
			var resourceInstance = (TResource) instance;

			// TODO configurable hot-reload outside of debug builds?
		#if DEBUG
			void WatcherChanged(object s, FileSystemEventArgs e) {
				// TODO confirm if dependency handling works
				if(
					(e.FullPath == realPath || resourceInstance.Dependencies.Any(dependency => dependency.FilePath == e.FullPath))
					&& !_reloadLocks.GetValueOrDefault(realPath, false)
				) {
					_reloadLocks[realPath] = true;
					
					platform.PlatformThread.ScheduleAction(() => {
						resourceInstance.Reload();
						_reloadLocks[realPath] = false;
					});
				}
			}
			
			if(realPath is not null) {
				if(!_resourceWatchers.TryGetValue(@namespace, out var watcher)) {
					watcher = new FileSystemWatcher(baseDirectory) {
						EnableRaisingEvents = true,
						NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
						IncludeSubdirectories = true
					};

					_resourceWatchers[@namespace] = watcher;
				}

				watcher.Changed += WatcherChanged;
				watcher.Created += WatcherChanged;
				watcher.Renamed += WatcherChanged;
			}
		#endif
			
			_loadedResources[metaPath] = resourceInstance;
			
			resourceInstance.Freed += _ => {
			#if DEBUG
				if(realPath is not null) {
					if(_resourceWatchers.TryGetValue(@namespace, out var watcher)) {
						watcher.Changed -= WatcherChanged;
						watcher.Created -= WatcherChanged;
						watcher.Renamed -= WatcherChanged;
					}
				}
			#endif
				
				_loadedResources.Remove(metaPath);
				_logger.Trace("Removed resource {Path} from loaded resources as it's no longer valid", realPath);
			};
			
			_logger.Debug("Successfully loaded resource of type {Type} at {Path}", typeof(TResource).Name, realPath);
			return new(resourceInstance);
		}
	}
}
