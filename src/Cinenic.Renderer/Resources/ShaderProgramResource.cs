using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Cinenic.Renderer.Shader;
using Cinenic.Resources;
using NLog;

namespace Cinenic.Renderer.Resources {
	
	public class ShaderProgramResource : Resource<ShaderProgramResource.Import> {

		static ShaderProgramResource() {
			ResourceRegistry.RegisterFormat<ShaderProgramResource, Import>();
		}
		
		public override Type SettingsType => typeof(Import);

		public List<Ref<ShaderResource>> Shaders { get; private set; }
		public ShaderProgram? Program { get; private set; }

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		public void Create(ShaderProgram program) {
			Platform = program.Platform;
			Settings = new Import();
			Id = Settings.Id;

			Program = program;
		}
		
		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings) {
			base.Load(platform, filePath, stream, resourceAssembly, settings);

			var file = JsonSerializer.Deserialize<File>(stream, ImportSettings.DefaultSerializerOptions);
			Debug.Assert(file is not null);
			
			// load required shaders
			if(!file.ShaderPaths.TryGetValue(platform.Identifier, out var shaderPaths)) {
				throw new IncompatiblePlatformException($"This shader program is incompatible with the current platform {platform.Identifier}");
			}

			Shaders = [];

			foreach(var shaderPath in shaderPaths) {
				string realPath = "";
				bool explicitPath = false;

				if(shaderPath.StartsWith('/')) {
					realPath = shaderPath;
				} else {
					realPath = Path.GetDirectoryName(filePath) + Path.DirectorySeparatorChar + shaderPath;
					explicitPath = true;
				}
				
				var shader = ResourceManager.Load<ShaderResource>(
					platform,
					realPath,
					explicitPath: explicitPath,
					assembly: resourceAssembly
				)!;
				
				Shaders.Add(shader);
				Dependencies.Add(shader.Get());
			}
			
			// create program
			Program = ShaderProgram.Create(
				platform,
				Shaders.ConvertAll(s => s.Get().Shader!).ToArray()
			);
		}
		
		public override void Dispose(bool reloading) {
			Program?.Dispose();

			foreach(var shader in Shaders) {
				shader.Dispose();
			}
			
			base.Dispose(reloading);
		}

		public static implicit operator ShaderProgram(ShaderProgramResource resource) => resource.Program;
		
		public class Import : ImportSettings {

			public override string FormatId => "shader_program";
		}
		
		public class File {
			
			public Dictionary<Platform, List<string>> ShaderPaths { get; set; } = [];
		}
		
		[Serializable]
		public class IncompatiblePlatformException : Exception {
			
			public IncompatiblePlatformException(string? message) : base(message) { }
			public IncompatiblePlatformException(string? message, Exception? innerException) : base(message, innerException) { }
		}
	}
}
