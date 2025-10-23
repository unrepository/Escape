using Escape.Renderer;
using Escape.Renderer.Resources;
using Escape.Renderer.Shader;
using Escape.Resources;
using Silk.NET.Vulkan;

namespace Escape.Extensions.Debugging {
	
	internal class EmptyShaderPipeline : IShaderPipeline {
		
		public IPlatform Platform { get; }
		public Ref<ShaderProgramResource> Program { get; }
		
		public DescriptorSet VkTexturesDescriptor { get; }

		public EmptyShaderPipeline(IPlatform platform) {
			Platform = platform;
			Program = ResourceManager.Load<ShaderProgramResource>(platform, "/shader_programs/empty.program")!;
		}
		
		public void PushData() { }
		
		public void Dispose() {
			GC.SuppressFinalize(this);
			Program.Dispose();
		}
	}
}
