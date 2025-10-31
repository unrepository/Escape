using System.Diagnostics;
using System.Reflection;
using Escape.Renderer;
using NLog;

namespace Escape.Resources {
	
	public static class ResourceRegistry {

		public static Dictionary<string, Format> Formats { get; } = [];

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
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
			
			var newCtor = type.GetConstructor([ typeof(IPlatform), typeof(string), typeof(TResourceValue), typeof(TImportSettings) ]);
			
			if(newCtor is null) {
				_logger.Warn("Format {FormatId} has no new constructor; it will not be able to be created", importMeta.FormatId);
			}

			var valueCtor = typeof(TResourceValue).GetConstructor(BindingFlags.Public | BindingFlags.Instance, []);

			/*if(valueCtor is null) {
				_logger.Warn("Format {FormatId} has no default constructor for its value; it will not be able to be constructed", importMeta.FormatId);
			}*/
			
			Formats[importMeta.FormatId] = new Format {
				FormatId = importMeta.FormatId,
				FileExtensions = defaultResource.FileExtensions,
				DefaultConstructor = defaultCtor,
				NewConstructor = newCtor,
				ValueConstructor = valueCtor,
				LoadMethod = loadMethod,
				ResourceType = typeof(TResource),
				ValueType = typeof(TResourceValue),
				MetadataType = typeof(TImportSettings)
			};
		}

		public static Format? GetFormat(string type) {
			if(Formats.TryGetValue(type, out var format)) {
				return format;
			}

			return null;
		}
		
		public static Format? GetFormatByExtension(string fileExtension) {
			foreach(var (_, format) in Formats) {
				if(format.FileExtensions.Contains(fileExtension)) return format;
			}

			return null;
		}

		public struct Format {
			
			public required string FormatId { get; init; }
			public required string[] FileExtensions { get; init; }
			public required ConstructorInfo DefaultConstructor { get; init; }
			public required ConstructorInfo? NewConstructor { get; init; }
			public required ConstructorInfo? ValueConstructor { get; init; }
			public required MethodInfo LoadMethod { get; init; }
			public required Type ResourceType { get; init; }
			public required Type ValueType { get; init; }
			public required Type MetadataType { get; init; }
		}
	}
}
