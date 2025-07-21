using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cinenic.Resources {
	
	public abstract class ImportMetadata {

		public const string FileExtension = ".meta.json";

		public static readonly JsonSerializerOptions DefaultSerializerOptions = new() {
			IndentCharacter = '\t',
			IndentSize = 1,
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			ReadCommentHandling = JsonCommentHandling.Skip,
			Converters = {
				new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.SnakeCaseLower, false)
			}
		};

		public Guid Id { get; init; } = Guid.NewGuid();
		public abstract string FormatId { get; }
		
		//public MD5? FileHash { get; set; }
	}
}
