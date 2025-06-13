using System.Drawing;
using GENESIS.GPU.OpenGL;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GENESIS.GPU {
	
	public abstract class Texture : IDisposable {
		
		public IPlatform Platform { get; }
		
		public ulong Handle { get; protected set; }
		public Vector2D<uint> Size { get; }

		protected Texture(IPlatform platform, Vector2D<uint> size, Filter filter, WrapMode wrapMode) {
			Platform = platform;
			Size = size;
		}

		public abstract void LoadImage(Image<Rgba32> image);
		
		public abstract void Bind();
		public abstract void Unbind();

		public abstract void Dispose();

		public static Texture Create(IPlatform platform, Vector2D<uint> size,
		                              Filter filter = Filter.Linear,
		                              WrapMode wrapMode = WrapMode.Repeat)
		{
			return platform switch {
				GLPlatform glPlatform => new GLTexture(glPlatform, size, filter, wrapMode),
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}

		public static Texture Create(IPlatform platform, byte[] imageData,
		                             Filter filter = Filter.Linear,
		                             WrapMode wrapMode = WrapMode.Repeat)
		{
			using var image = Image.Load<Rgba32>(imageData);
			var texture = Create(platform, new Vector2D<uint>((uint) image.Width, (uint) image.Height), filter, wrapMode);
			texture.LoadImage(image);
			
			return texture;
		}
		
		public static Texture Create(IPlatform platform, Stream imageData,
		                             Filter filter = Filter.Linear,
		                             WrapMode wrapMode = WrapMode.Repeat)
		{
			using var image = Image.Load<Rgba32>(imageData);
			var texture = Create(platform, new Vector2D<uint>((uint) image.Width, (uint) image.Height), filter, wrapMode);
			texture.LoadImage(image);
			
			return texture;
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
