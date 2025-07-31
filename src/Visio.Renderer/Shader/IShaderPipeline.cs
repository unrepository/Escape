using Visio.Renderer.Vulkan;
using Visio.Resources;
using Silk.NET.Vulkan;
using Visio.Renderer.Resources;

namespace Visio.Renderer.Shader {
	
	public interface IShaderPipeline : IDisposable {

		public IPlatform Platform { get; }
		public Ref<ShaderProgramResource> Program { get; }

		public void PushData();
		
	#region Vulkan
		public DescriptorSet VkTexturesDescriptor { get; }
	#endregion
	}
}
