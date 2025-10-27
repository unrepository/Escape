using System.Reflection;
using Escape.Resources;

namespace Escape.Renderer.Resources {
	
	public class ShaderResource : Resource<Shader.Shader, ShaderResource.Import> {
		
		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".frag", ".vert", ".tesc", ".tese", ".geom", ".comp", ".glsl", ".shader" ];

		public Shader.Shader? Shader { get; private set; }

		public ShaderResource() { }
		public ShaderResource(IPlatform platform, string? filePath, Shader.Shader value, Import? settings = null) : base(platform, filePath, value, settings) { }
		
		public override void SaveNew() {
			throw new NotImplementedException();
		}

		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings, bool reloading = false) {
			base.Load(platform, filePath, stream, resourceAssembly, settings, reloading);
			
			// if(platform.Identifier != Settings.TargetPlatform) {
			// 	throw new InvalidPlatformException();
			// }

			using var reader = new StreamReader(stream);
			Shader = Renderer.Shader.Shader.Create(platform, Settings.Family, reader.ReadToEnd());
		}

		public override ShaderResource Duplicate() {
			using var stream = new FileStream(FilePath, FileMode.Open);

			var resource = new ShaderResource();
			resource.Load(Platform, FilePath, stream, ResourceAssembly, Settings);

			Duplicates.Add(resource);
			return resource;
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
