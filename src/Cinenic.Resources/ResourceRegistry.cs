using System.Diagnostics;
using System.Reflection;
using Cinenic.Renderer;

namespace Cinenic.Resources {
	
	public static class ResourceRegistry {

		private static Dictionary<string, (ConstructorInfo Constructor, MethodInfo LoadMethod, Type MetaType)> _formats = [];
		
		public static void RegisterFormat<TResource, TImportSettings>()
			where TResource : Resource<TImportSettings>, new()
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
			
			_formats[new TImportSettings().FormatId] = (defaultCtor, loadMethod!, typeof(TImportSettings));
		}

		public static (ConstructorInfo Constructor, MethodInfo LoadMethod, Type MetaType)? GetFormat(string type) {
			if(_formats.TryGetValue(type, out var format)) {
				return format;
			}

			return null;
		}
	}
}
