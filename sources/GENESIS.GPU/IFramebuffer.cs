using GENESIS.GPU.OpenGL;
using Silk.NET.Maths;

namespace GENESIS.GPU {

	public interface IFramebuffer : IDisposable {
		
		public uint Handle { get; }

		public Vector2D<uint> Size { get; }
		
		public void Bind();
		public void Unbind();

		public void AttachTexture(ITexture texture);

		public void Resize(Vector2D<int> size);
		
		public static IFramebuffer Create(GLPlatform platform, Vector2D<uint> size, GLTexture? baseTexture = null)
			=> new GLFramebuffer(platform, size, baseTexture);
	}
}
