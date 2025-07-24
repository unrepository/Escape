using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Vulkan;
using Silk.NET.Maths;

namespace Cinenic.Renderer {

	public abstract class Framebuffer : IDisposable {
		
		public IPlatform Platform { get; }
		public RenderQueue Queue { get; }
		
		public ulong Handle { get; protected set; }
		public Vector2D<uint> Size { get; protected set; }

		public delegate void ResizeEventHandler(Vector2D<int> newSize);
		public event ResizeEventHandler? Resized;
		
		[Obsolete]
		protected List<Texture> TextureAttachments { get; } = [];

		protected Framebuffer(IPlatform platform, RenderQueue queue, Vector2D<uint> size) {
			Platform = platform;
			Queue = queue;
			Size = size;
		}

		[Obsolete]
		public IReadOnlyList<Texture> GetTextureAttachments()
			=> TextureAttachments;
		
		[Obsolete("Use framebuffer as parameter in RenderPipeline.Begin()")]
		public abstract void Bind();
		[Obsolete]
		public abstract void Unbind();

		public abstract void CreateAttachment(AttachmentType type);
		public abstract void Create();
		
		[Obsolete("Use CreateAttachment()")]
		public abstract void AttachTexture(Texture texture);
		
		public abstract void Resize(Vector2D<int> size);

		public abstract byte[] Read(int attachment = 0, Rectangle<uint>? area = null);
		
		public abstract void Dispose();

		protected void OnResized(Vector2D<int> newSize) {
			Resized?.Invoke(newSize);
		}

		public static Framebuffer Create(
			IPlatform platform,
			RenderQueue queue,
			Vector2D<uint> size
		) {
			return platform switch {
				GLPlatform glPlatform => new GLFramebuffer(glPlatform, queue, size),
				VkPlatform vkPlatform => new VkFramebuffer(vkPlatform, queue, size),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}

		public enum AttachmentType {
			
			Color,
			Depth
		}
	}
}
