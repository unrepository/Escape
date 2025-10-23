using System.Reflection;
using Escape.Resources;

namespace Escape.Renderer.Resources {
	
	public class ShaderResource : Resource<ShaderResource.Import> {
		
		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".frag", ".vert", ".tesc", ".tese", ".geom", ".comp", ".glsl", ".shader" ];

		public Shader.Shader? Shader { get; private set; }

		public void Create(Shader.Shader shader) {
			Platform = shader.Platform;
			Settings = new Import();
			Id = Settings.Id;
			
			Shader = shader;
		}
		
		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings) {
			base.Load(platform, filePath, stream, resourceAssembly, settings);
			
			// if(platform.Identifier != Settings.TargetPlatform) {
			// 	throw new InvalidPlatformException();
			// }

			using var reader = new StreamReader(stream);
			Shader = Renderer.Shader.Shader.Create(platform, Settings.Family, reader.ReadToEnd());
		}

		public override void Dispose(bool reloading) {
			Shader?.Dispose();
			base.Dispose(reloading);
		}

		public static implicit operator Shader.Shader(ShaderResource resource) => resource.Shader;

		public class Import : ImportMetadata {

			public override string FormatId => "shader";

			//public Platform TargetPlatform { get; set; } = Renderer.Platform.Vulkan;
			public Shader.Shader.Family Family { get; set; } = Renderer.Shader.Shader.Family.Vertex;
		}
	}
}
