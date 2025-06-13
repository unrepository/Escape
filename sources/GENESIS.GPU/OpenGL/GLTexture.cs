using System.Diagnostics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ARB;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GENESIS.GPU.OpenGL {
	
	public class GLTexture : Texture {

		public uint Id { get; private set; }
		
		private readonly GLPlatform _platform;
		private readonly ArbBindlessTexture _bindlessTexture;

		public GLTexture(GLPlatform platform, Vector2D<uint> size, Filter filter, WrapMode wrapMode)
			: base(platform, size, filter, wrapMode)
		{
			_platform = platform;
			_bindlessTexture = new(_platform.API.Context);

			Id = platform.API.GenTexture();
			Bind();

			/*unsafe {
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
			}*/
			
			_platform.API.TextureStorage2D(
				Id,
				1,
				SizedInternalFormat.Rgb8,
				size.X,
				size.Y
			);

			uint glFilter = filter switch {
				Filter.Nearest => (uint) GLEnum.Nearest,
				Filter.Linear => (uint) GLEnum.Linear
			};
			
			uint glWrapMode = wrapMode switch {
				WrapMode.Clamp => (uint) GLEnum.ClampToEdge,
				WrapMode.Repeat => (uint) GLEnum.Repeat,
				WrapMode.RepeatMirrored => (uint) GLEnum.MirroredRepeat
			};

			/*float[] borderColor = [ 0.0f, 0.0f, 0.0f, 0.0f ];

			unsafe {
				fixed(float* ptr = &borderColor[0]) {
					platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, ptr);
				}
			}*/
			
			platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, glFilter);
			platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, glFilter);
			platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, glWrapMode);
			platform.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, glWrapMode);
			
			Unbind();
		}

		public unsafe override void LoadImage(Image<Rgba32> image) {
			/*_platform.API.TextureStorage2D(
				Id,
				1,
				SizedInternalFormat.Rgb8,
				(uint) image.Width,
				(uint) image.Height
			);*/
			
			image.ProcessPixelRows(accessor => {
				var data = new Rgba32*[image.Height];
				
				for(int y = 0; y < accessor.Height; y++) {
					fixed(Rgba32* addr = accessor.GetRowSpan(y)) {
						data[y] = addr;
					}
				}
				
				_platform.API.TextureSubImage2D(
					Id,
					0,
					0,
					0,
					(uint) image.Width,
					(uint) image.Height,
					PixelFormat.Rgb,
					PixelType.UnsignedByte,
					data[0]
				);
			});
			
			//_platform.API.GenerateTextureMipmap(Id);
			Handle = _bindlessTexture.GetTextureHandle(Id);
			
			Debug.Assert(Handle != 0);
		}

		public override void Bind() {
			Debug.Assert(Id != 0);
			_platform.API.BindTexture(TextureTarget.Texture2D, Id);
		}
		
		public override void Unbind() {
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
