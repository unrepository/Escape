using System.Diagnostics;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Eclair.Renderer.OpenGL {
	
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

			var data = new Rgba32[image.Width * image.Height];
			
			image.ProcessPixelRows(accessor => {
				for(int y = 0; y < image.Height; y++) {
					fixed(Rgba32* addr = accessor.GetRowSpan(image.Height - y - 1)) {
						for(int x = 0; x < image.Width; x++) {
							data[y * image.Width + x] = addr[x];
						}
					}
				}
			});

			fixed(void* ptr = &data[0]) {
				_platform.API.TexImage2D(
					TextureTarget.Texture2D,
					0,
					InternalFormat.Rgba8,
					(uint) image.Width,
					(uint) image.Height,
					0,
					PixelFormat.Rgba,
					PixelType.UnsignedByte,
					ptr
				);
			}
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
