using System.Security.Cryptography;
using System.Text.Json;

namespace Cinenic.Resources {
	
	public abstract class ImportSettings {

		public const string FileExtension = ".import.json";

		public static readonly JsonSerializerOptions DefaultSerializerOptions = new() {
			IndentCharacter = '\t',
			IndentSize = 1,
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			ReadCommentHandling = JsonCommentHandling.Skip
		};

		public Guid Id { get; init; } = Guid.NewGuid();
		public abstract string Type { get; }
		
		public MD5? FileHash { get; set; }
	}
}
