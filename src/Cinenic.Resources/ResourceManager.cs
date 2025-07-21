using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer;
using NLog;

namespace Cinenic.Resources {
	
	public static class ResourceManager {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private static readonly Dictionary<string, object> _loadedResources = [];
		private static readonly Dictionary<Assembly, FileSystemWatcher> _resourceWatchers = [];
		private static readonly ConcurrentDictionary<string, bool> _reloadLocks = [];
		
		public static Ref<TResource>? Load<TResource>(IPlatform platform, string path, bool required = true)
			where TResource : class, IResource, new()
		{
			Debug.Assert(!Path.IsPathFullyQualified(path), "Resource path must be relative to the current assembly");
			
			string callingDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)!;
			string absolutePath = callingDirectory + Path.DirectorySeparatorChar + path;
			
			if(_loadedResources.TryGetValue(absolutePath, out var loadedResource)) {
				Debug.Assert(loadedResource is TResource);
				
				_logger.Debug("Returning already-loaded resource for {FilePath}", absolutePath);
				return new((TResource) loadedResource);
			}
			
			if(!File.Exists(absolutePath)) {
				_logger.Warn("Resource file not found: {FilePath}", absolutePath);

				if(required) {
					throw new FileNotFoundException("Required resource file not found", absolutePath);
				}

				return null;
			}

			ImportSettings? importSettings = null;
			var settingsType = new TResource().SettingsType;

			if(File.Exists(absolutePath + ImportSettings.FileExtension)) {
				using var importStream = new FileStream(absolutePath + ImportSettings.FileExtension, FileMode.Open);
				importSettings = (ImportSettings?) JsonSerializer.Deserialize(importStream, settingsType, ImportSettings.DefaultSerializerOptions);
			}

			if(importSettings is null) {
				_logger.Debug("Creating a new default ImportSettings instance as an existing one could not be found");
				importSettings = (ImportSettings) settingsType.GetConstructor([]).Invoke(null);
			}

			var format = ResourceRegistry.GetFormat(importSettings.Type);

			if(format is null) {
				_logger.Warn("Could not find import format for resource type {Type}", importSettings.Type);

				if(required) {
					throw new TargetException($"Could not find import format for resource type {importSettings.Type}");
				}

				return null;
			}

			using var fileStream = new FileStream(absolutePath, FileMode.Open);

			var instance = format.Value.Constructor.Invoke(null);
			format.Value.LoadMethod.Invoke(
				instance,
				[ platform, absolutePath, fileStream, importSettings ]
			);
			
			Debug.Assert(instance is not null);
			Debug.Assert(instance is TResource);
			var resourceInstance = (TResource) instance;

			if(!_resourceWatchers.TryGetValue(Assembly.GetCallingAssembly(), out var watcher)) {
				watcher = new FileSystemWatcher(callingDirectory) {
					EnableRaisingEvents = true,
					NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
					IncludeSubdirectories = true
				};

				_resourceWatchers[Assembly.GetCallingAssembly()] = watcher;
			}

			void WatcherChanged(object s, FileSystemEventArgs e) {
				if(e.FullPath == absolutePath && !_reloadLocks.GetValueOrDefault(absolutePath, false)) {
					_reloadLocks[absolutePath] = true;
					
					platform.PlatformThread.ScheduleAction(() => {
						resourceInstance.Reload();
						_reloadLocks[absolutePath] = false;
					});
				}
			}

			watcher.Changed += WatcherChanged;
			watcher.Created += WatcherChanged;
			watcher.Renamed += WatcherChanged;
			
			_loadedResources[absolutePath] = resourceInstance;
			
			resourceInstance.Freed += _ => {
				if(_resourceWatchers.TryGetValue(Assembly.GetCallingAssembly(), out var watcher)) {
					watcher.Changed -= WatcherChanged;
					watcher.Created -= WatcherChanged;
					watcher.Renamed -= WatcherChanged;
				}
				
				_loadedResources.Remove(absolutePath);
				_logger.Trace("Removed resource {Path} from loaded resources as it's no longer valid", absolutePath);
			};
			
			_logger.Debug("Successfully loaded resource of type {Type} at {Path}", typeof(TResource).Name, absolutePath);
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
