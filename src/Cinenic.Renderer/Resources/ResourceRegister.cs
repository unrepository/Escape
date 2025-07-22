using System.Runtime.CompilerServices;
using Cinenic.Resources;

namespace Cinenic.Renderer.Resources {
	
	public static class ResourceRegister {

		[ModuleInitializer]
		public static void Initialize() {
			ResourceRegistry.RegisterFormat<TextureResource, TextureResource.Import>();
			ResourceRegistry.RegisterFormat<ShaderResource, ShaderResource.Import>();
			ResourceRegistry.RegisterFormat<ShaderProgramResource, ShaderProgramResource.Import>();
		}
	}
}
