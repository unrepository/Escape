using System.Reflection;
using System.Text.Json;
using Arch.Core;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Core.Resources {
	
	public class SceneResource : Resource<Scene, SceneResource.Import> {

		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".scene" ];

		private static readonly JsonDocumentOptions _documentOptions = new() {
			CommentHandling = JsonCommentHandling.Skip
		};
		
		private static readonly JsonSerializerOptions _serializerOptions = new(ImportMetadata.DefaultSerializerOptions);

		static SceneResource() {
			_serializerOptions.Converters.Add(new WorldConverter());
		}
		
		public SceneResource() { }
		public SceneResource(IPlatform platform, string? filePath, Scene value, Import? settings = null) : base(platform, filePath, value, settings) { }

		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings, bool reloading = false) {
			base.Load(platform, filePath, stream, resourceAssembly, settings, reloading);
			
			var document = JsonDocument.Parse(stream, _documentOptions);
			var root = document.RootElement;

			var id = root.GetProperty("id").GetString();
			var world = root.GetProperty("world").Deserialize<World>(_serializerOptions);

			Value = new Scene(platform, id ?? "", world, null);
		}

		public override bool Save(bool metadataOnly = true) {
			if(!base.Save(metadataOnly)) return false;
			if(metadataOnly) return true;

			using var stream = new FileStream(FilePath!, FileMode.Create, FileAccess.Write);
			var writer = new Utf8JsonWriter(stream);

			writer.WriteStartObject();
			writer.WriteString("id", Value.Id);
			writer.WritePropertyName("world");
			new WorldConverter().Write(writer, Value.World, _serializerOptions);
			writer.WriteEndObject();
			
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
