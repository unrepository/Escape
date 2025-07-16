using Silk.NET.Maths;
using Silk.NET.Vulkan;

// TODO
namespace Cinenic.Renderer.Vulkan {
	
	public class VkFramebuffer : Framebuffer {
		
		public Silk.NET.Vulkan.Framebuffer Base { get; init; }
		
		public SwapchainKHR Swapchain { get; protected set; }

		public Format SwapchainFormat { get; protected set; }
		public Extent2D SwapchainExtent { get; protected set; }
		public Image[] SwapchainImages { get; protected set; }
		public ImageView[] SwapchainImageViews { get; protected set; }
		public Silk.NET.Vulkan.Framebuffer[] SwapchainFramebuffers { get; protected set; }

		public VkFramebuffer(VkPlatform platform, RenderQueue queue, Vector2D<uint> size) : base(platform, queue, size) {
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
			GC.SuppressFinalize(this);
		}
	}
}
