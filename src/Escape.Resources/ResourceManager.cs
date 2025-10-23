using System.Collections.Concurrent;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using Escape.Extensions.CSharp;
using NLog;
using Escape.Renderer;

namespace Escape.Resources {
	
	public static class ResourceManager {

		public const string ASSETS_DIRECTORY_NAME = "assets";
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private static readonly Dictionary<Assembly, ResourceDatabase> _databases = [];
		
		private static readonly Dictionary<string, object> _loadedResources = [];
		private static readonly Dictionary<Assembly, FileSystemWatcher> _resourceWatchers = [];
		private static readonly ConcurrentDictionary<string, bool> _reloadLocks = [];

		static ResourceManager() {
			// dispose all remaining resources when process exits, primarily to save them
			AppDomain.CurrentDomain.ProcessExit += (_, _) => {
				foreach(var resourceObject in _loadedResources.Values) {
					((IDisposable) resourceObject).Dispose();
				}

				// save databases only in debug builds, as they will primarily be done through the resource editor
			#if DEBUG
				foreach(var (assembly, database) in _databases) {
					database.Save();
					_logger.Trace("Saved resource database for {Assembly}", assembly.GetName().Name ?? assembly.GetName().FullName);
				}
			#endif
			};

			/*AppDomain.CurrentDomain.UnhandledException += (_, _) => {
				foreach(var resourceObject in _loadedResources.Values) {
					((IDisposable) resourceObject).Dispose();
				}
			};*/
		}

		public static Ref<TResource>? Load<TResource>(
			IPlatform platform,
			string resource,
			bool required = true,
			bool explicitPath = false,
			Assembly? assembly = null
		)
			where TResource : class, IResource, new()
		{
			assembly ??= Assembly.GetCallingAssembly();
			
			if(Guid.TryParse(resource, out var guid)) {
				return LoadById<TResource>(platform, guid, required, assembly);
			}
			
			return LoadByPath<TResource>(platform, resource, required, explicitPath, assembly);
		}
		
		public static Ref<TResource>? LoadById<TResource>(
			IPlatform platform,
			Guid id,
			bool required = true,
			Assembly? assembly = null
		)
			where TResource : class, IResource, new()
		{
			assembly ??= Assembly.GetCallingAssembly();
			
			var @namespace = _GetNamespace(assembly);
			var database = LoadDatabase(assembly);

			if(database is null || !database.Entries.TryGetValue(id, out var entry)) {
				_logger.Warn("Could not load resource with id {Id} from database", id);

				if(required) {
					throw new KeyNotFoundException($"Could not load resource with id {id} from database");
				}

				return null;
			}

			return LoadByPath<TResource>(
				platform,
				entry.MetadataPath.ReplaceLast(ImportMetadata.FILE_EXTENSION, "", StringComparison.OrdinalIgnoreCase),
				required,
				assembly: assembly
			);
		}
		
		public static Ref<TResource>? LoadByPath<TResource>(
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
			var @namespace = _GetNamespace(assembly);

			if(@namespace == "unknown") {
				_logger.Warn("Assembly has no name? Resource loading might break!");
				_logger.Warn("Namespace set to \"unknown\"");
			}

			var baseDirectory = _GetBaseDirectory(assembly);
			
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
			var metadataType = tempInstance.MetadataType;
			var fileExtensions = tempInstance.FileExtensions;
			
			ImportMetadata? importMeta = null;

			if(File.Exists(metaPath)) {
				importMeta = ImportMetadata.Load(metaPath, metadataType);
			}
			
			if(importMeta is null) {
				_logger.Debug("Creating a new default ImportSettings instance as an existing one could not be found");
				importMeta = (ImportMetadata) metadataType.GetConstructor([]).Invoke(null);
			}

			importMeta.Path = metaPath;
			
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
				if(!_resourceWatchers.TryGetValue(assembly, out var watcher)) {
					watcher = new FileSystemWatcher(baseDirectory) {
						EnableRaisingEvents = true,
						NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
						IncludeSubdirectories = true
					};

					_resourceWatchers[assembly] = watcher;
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
					if(_resourceWatchers.TryGetValue(assembly, out var watcher)) {
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

		public static ResourceDatabase? LoadDatabase(Assembly assembly) {
			if(_databases.TryGetValue(assembly, out var database)) return database;

			var baseDirectory = _GetBaseDirectory(assembly);
			var dbFilePath = baseDirectory + Path.DirectorySeparatorChar + ResourceDatabase.FILE_NAME;
			
			// load exiting database in debug only, as new resources don't get added automatically
		#if DEBUG
			if(File.Exists(dbFilePath)) {
				// if we have the db file, read from it
				database = ResourceDatabase.Load(dbFilePath);

				if(database is null) {
					_logger.Warn(
						"Could not read the database file for {Assembly} at {Path}",
						assembly.GetName().Name ?? assembly.GetName().FullName, dbFilePath
					);

					return null;
				}
			} else
		#endif
			{
				// otherwise, scan the assets directory for files and create a new database
				database = new ResourceDatabase();

				foreach(
					var metaFilePath in Directory.EnumerateFiles(
						baseDirectory,
						"*" + ImportMetadata.FILE_EXTENSION,
						SearchOption.AllDirectories
					)
				) {
					// read metadata file
					
					// very hacky, but first we need to read the metadata into dynamic to read the true format
					using var stream = new FileStream(metaFilePath, FileMode.Open);
					dynamic? dynamicMeta = JsonSerializer.Deserialize<ExpandoObject>(stream, ImportMetadata.DefaultSerializerOptions);
					
					if(dynamicMeta is null) {
						_logger.Warn(
							"Could not read metadata file {Path} for creating the {Assembly} database",
							metaFilePath, assembly.GetName().Name ?? assembly.GetName().FullName
						);
						
						continue;
					}
					
					var format = ResourceRegistry.GetFormat((string) dynamicMeta.format_id.ToString());

					if(format is null) {
						_logger.Warn("Could not find import format for resource type {Type}", dynamicMeta.format_id);
						continue;
					}
					
					// now we can read the actual type
					var importMeta = ImportMetadata.Load(metaFilePath, format.Value.MetaType);
					Debug.Assert(importMeta is not null, "This should never happen actually");

					database.Entries[importMeta.Id] = new ResourceDatabase.Entry {
						MetadataPath = metaFilePath.Replace(baseDirectory, "")
					};
				}
			}

			database.Path = dbFilePath;
			_databases[assembly] = database;
			return database;
		}

		public static string ResolvePath(string? baseDirectory, string path, out bool explicitPath) {
			if(Guid.TryParse(path, out _)) {
				explicitPath = false;
				return path;
			}

			if(!path.StartsWith('/')) {
				Debug.Assert(baseDirectory is not null, "path is relative but baseDirectory is null!");
				explicitPath = true;
				return baseDirectory + Path.DirectorySeparatorChar + path;
			}

			explicitPath = false;
			return path;
		}

		private static string _GetNamespace(Assembly assembly) => assembly.GetName().Name ?? "unknown";

		private static string _GetBaseDirectory(Assembly assembly) {
			var baseDirectory = Path.GetDirectoryName(assembly.Location)!;
			baseDirectory += Path.DirectorySeparatorChar + ASSETS_DIRECTORY_NAME;
			baseDirectory += Path.DirectorySeparatorChar + _GetNamespace(assembly);

			return baseDirectory;
		}
	}
}
