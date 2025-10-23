using Escape.Renderer.Resources;
using Escape.Renderer.Vulkan;
using Escape.Resources;
using Silk.NET.Vulkan;

namespace Escape.Renderer.Shader {
	
	public interface IShaderPipeline : IDisposable {

		public IPlatform Platform { get; }
		public Ref<ShaderProgramResource> Program { get; }

		public void PushData();
		
	#region Vulkan
		public DescriptorSet VkTexturesDescriptor { get; }
	#endregion
	}
}
