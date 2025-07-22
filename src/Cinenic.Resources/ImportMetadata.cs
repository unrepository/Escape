using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cinenic.Resources {
	
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
		public string MetaPath { get; set; }
		
		public Guid Id { get; init; } = Guid.NewGuid();
		public abstract string FormatId { get; }

		public string? File { get; set; } = null;
		public byte[]? Data { get; set; } = null;
		//public MD5? FileHash { get; set; }
	}
}
