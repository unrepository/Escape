using Silk.NET.Maths;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkFramebuffer : Framebuffer {
		
		public Silk.NET.Vulkan.Framebuffer Base { get; init; }
		
		public SwapchainKHR Swapchain { get; init; }

		public Format SwapchainFormat { get; init; }
		public Extent2D SwapchainExtent { get; init; }
		public Image[] SwapchainImages { get; init; }
		public ImageView[] SwapchainImageViews { get; init; }
		public Silk.NET.Vulkan.Framebuffer[] SwapchainFramebuffers { get; init; }

		public VkFramebuffer(IPlatform platform, Vector2D<uint> size) : base(platform, size) {
			//throw new NotImplementedException();
		}
		
		public override void Bind() {
			throw new NotImplementedException();
		}
		
		public override void Unbind() {
			throw new NotImplementedException();
		}

		public override void Create() {
			throw new NotImplementedException();
		}

		public override void AttachTexture(Texture texture) {
			throw new NotImplementedException();
		}
		
		public override void Resize(Vector2D<int> size) {
			throw new NotImplementedException();
		}

		public override byte[] Read(int attachment = 0, Rectangle<uint>? area = null) {
			throw new NotImplementedException();
		}
		
		public override void Dispose() {
			//throw new NotImplementedException();
		}
	}
}
