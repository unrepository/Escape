using Cinenic.Renderer.Vulkan;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Shader {
	
	public interface IShaderPipeline : IDisposable {

		public IPlatform Platform { get; }
		public ShaderProgram Program { get; }

		public void PushData();
		
	#region Vulkan
		public DescriptorSet VkTexturesDescriptor { get; }
		public void VkBindTextureUnit(uint unit, ImageView imageView, Sampler sampler);
	#endregion
	}
}
