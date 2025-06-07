using System.Drawing;
using GENESIS.GPU.OpenGL;
using Silk.NET.Maths;

namespace GENESIS.GPU {
	
	public abstract class Texture : IDisposable {
		
		public uint Handle { get; protected set; }
		public Vector2D<uint> Size { get; }

		protected Texture(Vector2D<uint> size, Filter filter, WrapMode wrapMode) {
			Size = size;
		}
		
		public abstract void Bind();
		public abstract void Unbind();

		public abstract void Dispose();

		public static Texture Create(Platform platform, Vector2D<uint> size,
		                              Filter filter = Filter.Linear,
		                              WrapMode wrapMode = WrapMode.Clamp)
		{
			return platform switch {
				GLPlatform glPlatform => new GLTexture(glPlatform, size, filter, wrapMode),
				_ => throw new NotImplementedException()
			};
		}

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
