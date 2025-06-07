using GENESIS.GPU.OpenGL;
using Silk.NET.Maths;

namespace GENESIS.GPU {

	public abstract class Framebuffer : IDisposable {
		
		public uint Handle { get; protected set; }
		public Vector2D<uint> Size { get; protected init; }

		protected Framebuffer(Vector2D<uint> size) {
			Size = size;
		}
		
		public abstract void Bind();
		public abstract void Unbind();

		public abstract void AttachTexture(Texture texture);
		public abstract void Resize(Vector2D<int> size);

		public abstract void Dispose();

		public static Framebuffer Create(IPlatform platform,
		                                 Vector2D<uint> size,
		                                 Texture? baseTexture = null) {
			return platform switch {
				GLPlatform glPlatform => new GLFramebuffer(glPlatform, size, baseTexture as GLTexture),
				_ => throw new NotImplementedException()
			};
		}
	}
}
