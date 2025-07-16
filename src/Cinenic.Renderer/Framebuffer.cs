using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Vulkan;
using Silk.NET.Maths;

namespace Cinenic.Renderer {

	public abstract class Framebuffer : IDisposable {
		
		public IPlatform Platform { get; }
		public RenderQueue Queue { get; }
		
		public uint Handle { get; protected set; }
		public Vector2D<uint> Size { get; protected init; }

		protected List<Texture> TextureAttachments { get; } = [];

		protected Framebuffer(IPlatform platform, RenderQueue queue, Vector2D<uint> size) {
			Platform = platform;
			Queue = queue;
			Size = size;
		}

		public IReadOnlyList<Texture> GetTextureAttachments()
			=> TextureAttachments;
		
		[Obsolete("Use framebuffer as parameter in RenderPipeline.Begin()")]
		public abstract void Bind();
		[Obsolete]
		public abstract void Unbind();

		public abstract void Create();
		public abstract void AttachTexture(Texture texture);
		public abstract void Resize(Vector2D<int> size);

		public abstract byte[] Read(int attachment = 0, Rectangle<uint>? area = null);
		
		public abstract void Dispose();

		public static Framebuffer Create(
			IPlatform platform,
			RenderQueue queue,
			Vector2D<uint> size
		) {
			return platform switch {
				GLPlatform glPlatform => new GLFramebuffer(glPlatform, queue, size),
				VkPlatform vkPlatform => new VkFramebuffer(vkPlatform, queue, size),
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}
	}
}
