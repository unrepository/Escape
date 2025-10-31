using System.Reflection;
using System.Text.Json;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Core.Resources {
	
	public class SceneResource : Resource<Scene, SceneResource.Import> {

		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".scene" ];

		private static readonly JsonSerializerOptions _serializerOptions = new(ImportMetadata.DefaultSerializerOptions);

		static SceneResource() {
			_serializerOptions.Converters.Add(new WorldConverter());
		}
		
		public SceneResource() { }
		public SceneResource(IPlatform platform, string? filePath, Scene value, Import? settings = null) : base(platform, filePath, value, settings) { }

		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings, bool reloading = false) {
			base.Load(platform, filePath, stream, resourceAssembly, settings, reloading);
			
			var scene = JsonSerializer.Deserialize<Scene>(stream, _serializerOptions);
			Value = scene ?? throw new InvalidDataException("Failed to deserialize scene");
		}

		public override bool Save(bool metadataOnly = true) {
			if(!base.Save(metadataOnly)) return false;
			if(metadataOnly) return true;

			using var stream = new FileStream(FilePath!, FileMode.Create, FileAccess.Write);
			JsonSerializer.Serialize(stream, Value, _serializerOptions);

			return true;
		}

		public override void SaveNew() {
			throw new NotImplementedException();
		}

		public override SceneResource Duplicate() {
			using var stream = new FileStream(FilePath, FileMode.Open);

			var resource = new SceneResource();
			resource.Load(Platform, FilePath, stream, ResourceAssembly, Settings);

			Duplicates.Add(resource);
			return resource;
		}

		public class Import : ImportMetadata {

			public override string FormatId => "scene";
		}
	}
}
