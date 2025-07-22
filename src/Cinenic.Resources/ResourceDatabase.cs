using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cinenic.Resources {
	
	public class ResourceDatabase {

		public const string FILE_NAME = "res.db.json";
		
		[JsonIgnore]
		public string Path { get; set; }

		public Dictionary<Guid, Entry> Entries { get; set; } = [];

		public static ResourceDatabase? Load(string path) {
			using var stream = new FileStream(path, FileMode.Open);
			return Load(stream);
		}

		public static ResourceDatabase? Load(Stream stream) {
			return JsonSerializer.Deserialize<ResourceDatabase>(stream, ImportMetadata.DefaultSerializerOptions);
		}
		
		public void Save() {
			var data = JsonSerializer.SerializeToUtf8Bytes(this, ImportMetadata.DefaultSerializerOptions);
			File.WriteAllBytes(Path, data);
		}
		
		public class Entry {
			
			public string MetadataPath { get; set; }
		}
	}
}
