using System.Diagnostics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GENESIS.GPU.OpenGL {
	
	public class GLTexture : Texture {

		private int _unit = 0;
		
		private readonly GLPlatform _platform;

		public GLTexture(GLPlatform platform, Vector2D<uint> size, Filter filter, WrapMode wrapMode)
			: base(platform, size, filter, wrapMode)
		{
			_platform = platform;

			Id = platform.API.GenTexture();
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

		public unsafe override void LoadImage(Image<Rgba32> image) {
			Bind();
			
			image.ProcessPixelRows(accessor => {
				var data = new Rgba32*[image.Height];
				
				for(int y = 0; y < accessor.Height; y++) {
					fixed(Rgba32* addr = accessor.GetRowSpan(y)) {
						data[y] = addr;
					}
				}
				
				_platform.API.TexImage2D(
					TextureTarget.Texture2D,
					0,
					InternalFormat.Rgba8,
					(uint) image.Width,
					(uint) image.Height,
					0,
					PixelFormat.Rgba,
					PixelType.UnsignedByte,
					data[0]
				);
			});
		}

		public override void Bind(int unit = 0) {
			Debug.Assert(Id != 0);
			
			_unit = unit;
			_platform.API.ActiveTexture((TextureUnit) ((int) TextureUnit.Texture0 + unit));
			_platform.API.BindTexture(TextureTarget.Texture2D, Id);
		}
		
		public override void Unbind() {
			_platform.API.ActiveTexture((TextureUnit) ((int) TextureUnit.Texture0 + _unit));
			_platform.API.BindTexture(TextureTarget.Texture2D, 0);
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
			
			Debug.Assert(Id != 0);
			_platform.API.DeleteTexture(Id);
			Id = 0;
		}
	}
}
