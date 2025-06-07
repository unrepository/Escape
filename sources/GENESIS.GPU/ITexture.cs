using GENESIS.GPU.OpenGL;
using Silk.NET.Maths;

namespace GENESIS.GPU {
	
	public interface ITexture : IDisposable {
		
		public uint Handle { get; }
		
		public Vector2D<uint> Size { get; }

		public void Bind();
		public void Unbind();

		public static ITexture Create(GLPlatform platform, Vector2D<uint> size,
		                              Filter filter = Filter.Linear,
		                              WrapMode wrapMode = WrapMode.Clamp)
			=> new GLTexture(platform, size, filter, wrapMode);

		public enum Filter {
			
			Linear,
			Nearest,
		}

		public enum WrapMode {
			
			Clamp,
			Repeat,
			RepeatMirrored,
		}
	}
}
