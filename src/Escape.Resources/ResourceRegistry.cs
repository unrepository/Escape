using System.Diagnostics;
using System.Reflection;
using Escape.Renderer;

namespace Escape.Resources {
	
	public static class ResourceRegistry {

		public static
			Dictionary<string,
				(string[] FileExtensions,
				ConstructorInfo Constructor,
				MethodInfo LoadMethod,
				Type ResourceType, Type ResourceValueType,
				Type MetaType)
			> Formats { get; } = [];
		
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
			
			Formats[importMeta.FormatId] = (defaultResource.FileExtensions, defaultCtor, loadMethod, type, typeof(TResourceValue), typeof(TImportSettings));
		}

		public static (string[] FileExtensions, ConstructorInfo Constructor, MethodInfo LoadMethod, Type ResourceType, Type ResourceValueType, Type MetaType)? GetFormat(string type) {
			if(Formats.TryGetValue(type, out var format)) {
				return format;
			}

			return null;
		}
		
		public static (string[] FileExtensions, ConstructorInfo Constructor, MethodInfo LoadMethod, Type ResourceType, Type ResourceValueType, Type MetaType)? GetFormatByExtension(string fileExtension) {
			foreach(var (_, format) in Formats) {
				if(format.FileExtensions.Contains(fileExtension)) return format;
			}

			return null;
		}
	}
}
