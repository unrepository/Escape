using System.Diagnostics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace GENESIS.GPU.OpenGL {
	
	public class GLTexture : Texture {

		private readonly GLPlatform _platform;

		public GLTexture(GLPlatform platform, Vector2D<uint> size, Filter filter, WrapMode wrapMode)
			: base(platform, size, filter, wrapMode)
		{
			_platform = platform;

			Handle = platform.API.GenTexture();
			Bind();

			unsafe {
				platform.API.TexImage2D(
					TextureTarget.Texture2D,
					0,
					InternalFormat.Rgba8,
					size.X,
					size.Y,
					0,
					PixelFormat.Rgba,
					GLEnum.UnsignedByte,
					null
				);
			}

			uint glFilter = filter switch {
				Filter.Nearest => (uint) GLEnum.Nearest,
				Filter.Linear => (uint) GLEnum.Linear
			};
			
			uint glWrapMode = wrapMode switch {
				WrapMode.Clamp => (uint) GLEnum.ClampToEdge,
				WrapMode.Repeat => (uint) GLEnum.Repeat,
				WrapMode.RepeatMirrored => (uint) GLEnum.MirroredRepeat
			};

			float[] borderColor = [ 0.0f, 0.0f, 0.0f, 0.0f ];

			unsafe {
				fixed(float* ptr = &borderColor[0]) {
					platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, ptr);
				}
			}
			
			platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, glFilter);
			platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, glFilter);
			platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, glWrapMode);
			platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, glWrapMode);
			
			Unbind();
		}
		
		public override void Bind() {
			Debug.Assert(Handle != 0);
			_platform.API.BindTexture(TextureTarget.Texture2D, Handle);
		}
		
		public override void Unbind() {
			_platform.API.BindTexture(TextureTarget.Texture2D, 0);
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
			
			Debug.Assert(Handle != 0);
			_platform.API.DeleteTexture(Handle);
			Handle = 0;
		}
	}
}
