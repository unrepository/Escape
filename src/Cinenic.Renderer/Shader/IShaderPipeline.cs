using Cinenic.Renderer.Resources;
using Cinenic.Renderer.Vulkan;
using Cinenic.Resources;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Shader {
	
	public interface IShaderPipeline : IDisposable {

		public IPlatform Platform { get; }
		public Ref<ShaderProgramResource> Program { get; }

		public void PushData();
		
	#region Vulkan
		public DescriptorSet VkTexturesDescriptor { get; }
	#endregion
	}
}
