using System.Diagnostics;
using System.Reflection;
using Escape.Renderer;

namespace Escape.Resources {
	
	public static class ResourceRegistry {

		private static readonly Dictionary<string, (string[] FileExtensions, ConstructorInfo Constructor, MethodInfo LoadMethod, Type ResourceType, Type MetaType)> _formats = [];
		
		public static void RegisterFormat<TResourceValue, TResource, TImportSettings>()
			where TResource : Resource<TResourceValue, TImportSettings>, new()
			where TImportSettings : ImportMetadata, new()
		{
			var type = typeof(TResource);
			var defaultCtor = type.GetConstructor([]);
			var loadMethod = type.GetMethod(
				"Load",
				[ typeof(IPlatform), typeof(string), typeof(Stream), typeof(Assembly), typeof(ImportMetadata) ]
			);
			
			Debug.Assert(defaultCtor is not null);
			Debug.Assert(loadMethod is not null);

			var importMeta = new TImportSettings();
			var defaultResource = (TResource) defaultCtor.Invoke(null);
			
			_formats[importMeta.FormatId] = (defaultResource.FileExtensions, defaultCtor, loadMethod!, type, typeof(TImportSettings));
		}

		public static (string[] FileExtensions, ConstructorInfo Constructor, MethodInfo LoadMethod, Type ResourceType, Type MetaType)? GetFormat(string type) {
			if(_formats.TryGetValue(type, out var format)) {
				return format;
			}

			return null;
		}
		
		public static (string[] FileExtensions, ConstructorInfo Constructor, MethodInfo LoadMethod, Type ResourceType, Type MetaType)? GetFormatByExtension(string fileExtension) {
			foreach(var (_, format) in _formats) {
				if(format.FileExtensions.Contains(fileExtension)) return format;
			}

			return null;
		}
	}
}
