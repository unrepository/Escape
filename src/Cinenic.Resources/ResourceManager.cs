using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Cinenic.Renderer;
using NLog;

namespace Cinenic.Resources {
	
	public static class ResourceManager {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private static readonly Dictionary<string, object> _loadedResources = [];
		
		public static Ref<TResource>? Load<TResource>(IPlatform platform, string path, bool required = true)
			where TResource : class, IResource
		{
			if(_loadedResources.TryGetValue(path, out var loadedResource)) {
				Debug.Assert(loadedResource is TResource);
				
				_logger.Debug("Returning already-loaded resource for {FilePath}", path);
				return new((TResource) loadedResource);
			}
			
			if(!File.Exists(path)) {
				_logger.Warn("Resource file not found: {FilePath}", path);

				if(required) {
					throw new FileNotFoundException("Required resource file not found", path);
				}

				return null;
			}

			ImportSettings? importSettings = null;

			if(File.Exists(path + ImportSettings.FileExtension)) {
				using var importStream = new FileStream(path + ImportSettings.FileExtension, FileMode.Open);
				importSettings = (ImportSettings?) JsonSerializer.Deserialize(importStream, TResource.SettingsType, ImportSettings.DefaultSerializerOptions);
			}

			if(importSettings is null) {
				_logger.Debug("Creating a new default ImportSettings instance as an existing one could not be found");
				importSettings = (ImportSettings) TResource.SettingsType.GetConstructor([]).Invoke(null);
			}

			var format = ResourceRegistry.GetFormat(importSettings.Type);

			if(format is null) {
				_logger.Warn("Could not find import format for resource type {Type}", importSettings.Type);

				if(required) {
					throw new TargetException($"Could not find import format for resource type {importSettings.Type}");
				}

				return null;
			}

			using var fileStream = new FileStream(path, FileMode.Open);

			var instance = format.Value.Constructor.Invoke(null);
			format.Value.LoadMethod.Invoke(
				instance,
				[ platform, path, fileStream, importSettings ]
			);
			
			Debug.Assert(instance is not null);
			Debug.Assert(instance is TResource);
			var resourceInstance = (TResource) instance;

			_loadedResources[path] = resourceInstance;
			resourceInstance.Freed += _ => {
				_loadedResources.Remove(path);
				_logger.Trace("Removed resource {Path} from loaded resources as it's no longer valid", path);
			};
			
			_logger.Debug("Successfully loaded resource of type {Type} at {Path}", typeof(TResource).Name, path);
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
