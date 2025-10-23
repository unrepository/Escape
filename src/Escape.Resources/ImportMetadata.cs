using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Escape.Resources {
	
	public abstract class ImportMetadata {

		public const string FILE_EXTENSION = ".meta.json";

		public static readonly JsonSerializerOptions DefaultSerializerOptions = new() {
			IndentCharacter = '\t',
			IndentSize = 1,
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			ReadCommentHandling = JsonCommentHandling.Skip,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			Converters = {
				new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.SnakeCaseLower, false)
			}
		};

		[JsonIgnore]
		public string Path { get; set; }
		
		public Guid Id { get; init; } = Guid.NewGuid();
		public abstract string FormatId { get; }

		public string? File { get; set; } = null;
		public byte[]? Data { get; set; } = null;
		//public MD5? FileHash { get; set; }
		
		public static ImportMetadata? Load(string path, Type type) {
			using var stream = new FileStream(path, FileMode.Open);
			return Load(stream, type);
		}

		public static ImportMetadata? Load(Stream stream, Type type) {
			return (ImportMetadata?) JsonSerializer.Deserialize(stream, type, DefaultSerializerOptions);
		}
		
		public void Save(Type type) {
			var data = JsonSerializer.SerializeToUtf8Bytes(this, type, DefaultSerializerOptions);
			System.IO.File.WriteAllBytes(Path, data);
		}
	}
}
