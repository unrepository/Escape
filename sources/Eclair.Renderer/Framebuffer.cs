using Eclair.Renderer.OpenGL;
using Silk.NET.Maths;

namespace Eclair.Renderer {

	public abstract class Framebuffer : IDisposable {
		
		public IPlatform Platform { get; }
		
		public uint Handle { get; protected set; }
		public Vector2D<uint> Size { get; protected init; }

		protected List<Texture> TextureAttachments { get; } = [];

		protected Framebuffer(IPlatform platform, Vector2D<uint> size) {
			Platform = platform;
			Size = size;
		}

		public IReadOnlyList<Texture> GetTextureAttachments()
			=> TextureAttachments;
		
		public abstract void Bind();
		public abstract void Unbind();

		public abstract void AttachTexture(Texture texture);
		public abstract void Resize(Vector2D<int> size);

		public abstract byte[] Read(int attachment = 0, Rectangle<uint>? area = null);
		
		public abstract void Dispose();

		public static Framebuffer Create(IPlatform platform,
		                                 Vector2D<uint> size,
		                                 Texture? baseTexture = null) {
			return platform switch {
				GLPlatform glPlatform => new GLFramebuffer(glPlatform, size, baseTexture as GLTexture),
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}
	}
}
