using System.Runtime.CompilerServices;
using Escape.Renderer.Shader;
using Escape.Resources;

namespace Escape.Renderer.Resources {
	
	public static class ResourceRegister {

		[ModuleInitializer]
		public static void Initialize() {
			ResourceRegistry.RegisterFormat<Texture, TextureResource, TextureResource.Import>();
			ResourceRegistry.RegisterFormat<Shader.Shader, ShaderResource, ShaderResource.Import>();
			ResourceRegistry.RegisterFormat<ShaderProgram, ShaderProgramResource, ShaderProgramResource.Import>();
		}
	}
}
