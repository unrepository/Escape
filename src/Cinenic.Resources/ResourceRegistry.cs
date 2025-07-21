using System.Diagnostics;
using System.Reflection;
using Cinenic.Renderer;

namespace Cinenic.Resources {
	
	public static class ResourceRegistry {

		private static Dictionary<string, (ConstructorInfo Constructor, MethodInfo LoadMethod/*, Type ImportSettingsType*/)> _formats = [];

		static ResourceRegistry() {
			RegisterFormat<TextureResource, TextureResource.Import>();
		}
		
		public static void RegisterFormat<TResource, TImportSettings>()
			where TResource : IResource<TImportSettings>, new()
			where TImportSettings : ImportSettings, new()
		{
			var type = typeof(TResource);
			var defaultCtor = type.GetConstructor([]);
			var loadMethod = type.GetMethod(
				"Load",
				[ typeof(IPlatform), typeof(string), typeof(Stream), typeof(ImportSettings) ]
			);
			
			Debug.Assert(defaultCtor is not null);
			Debug.Assert(loadMethod is not null);
			
			_formats[new TImportSettings().Type] = (defaultCtor, loadMethod!/*, typeof(TImportSettings)*/);
		}

		public static (ConstructorInfo Constructor, MethodInfo LoadMethod)? GetFormat(string type) {
			if(_formats.TryGetValue(type, out var format)) {
				return format;
			}

			return null;
		}
	}
}
