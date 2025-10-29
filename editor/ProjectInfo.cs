using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Escape.Resources;

namespace Escape.Editor {
	
	public class ProjectInfo {

		public const string FILE_NAME = "project.json";
		
		public string Name { get; set; }
		public string Namespace { get; set; }
		public string MainAssemblyName { get; set; }
		public string ResourcesDirectory { get; set; }
		public string OutputDirectory { get; set; }
		
		public static ProjectInfo? Load(string path) {
			using var stream = new FileStream(path, FileMode.Open);
			return Load(stream);
		}

		public static ProjectInfo? Load(Stream stream) {
			return JsonSerializer.Deserialize<ProjectInfo>(stream, ImportMetadata.DefaultSerializerOptions);
		}
	}
}
