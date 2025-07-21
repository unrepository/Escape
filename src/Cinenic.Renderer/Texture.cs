using System.Drawing;
using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Vulkan;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Cinenic.Renderer {
	
	public abstract class Texture : IDisposable {
		
		public IPlatform Platform { get; }
		
		public uint Id { get; protected set; }
		public ulong Handle { get; protected set; }
		
		public Vector2D<uint> Size { get; }
		public TextureFilter Filter { get; }
		public TextureWrapMode WrapMode { get; }
		public TextureFormat Format { get; }
		
		protected Texture(IPlatform platform, Vector2D<uint> size, TextureFilter filter, TextureWrapMode wrapMode, TextureFormat format) {
			Platform = platform;
			Size = size;
			Filter = filter;
			WrapMode = wrapMode;
			Format = format;
		}

		public abstract void LoadImage(Image<Rgba32> image);
		
		[Obsolete]
		public virtual void Bind(int unit = 0) { }

		public abstract void Bind(RenderQueue queue, uint unit);
		public abstract void Unbind();

		public abstract void Dispose();

		public static Texture Create(
			IPlatform platform, Vector2D<uint> size,
			TextureFilter filter = TextureFilter.Linear,
			TextureWrapMode wrapMode = TextureWrapMode.Repeat,
			TextureFormat format = TextureFormat.RGBA8
		) {
			return platform switch {
				GLPlatform glPlatform => new GLTexture(glPlatform, size, filter, wrapMode, format),
				VkPlatform vkPlatform => new VkTexture(vkPlatform, size, filter, wrapMode, format),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}

		public static Texture Create(
			IPlatform platform, byte[] imageData,
			TextureFilter filter = TextureFilter.Linear,
			TextureWrapMode wrapMode = TextureWrapMode.Repeat,
			TextureFormat format = TextureFormat.RGBA8
		) {
			using var image = Image.Load<Rgba32>(imageData);
			var texture = Create(platform, new Vector2D<uint>((uint) image.Width, (uint) image.Height), filter, wrapMode, format);
			texture.LoadImage(image);
			
			return texture;
		}
		
		public static Texture Create(
			IPlatform platform, Stream imageData,
			TextureFilter filter = TextureFilter.Linear,
			TextureWrapMode wrapMode = TextureWrapMode.Repeat,
			TextureFormat format = TextureFormat.RGBA8
		) {
			using var image = Image.Load<Rgba32>(imageData);
			var texture = Create(platform, new Vector2D<uint>((uint) image.Width, (uint) image.Height), filter, wrapMode, format);
			texture.LoadImage(image);
			
			return texture;
		}

		public enum TextureFilter {
			
			Linear,
			Nearest,
		}

		public enum TextureWrapMode {
			
			ClampToBorder,
			ClampToEdge,
			Repeat,
			RepeatMirrored,
		}

		public enum TextureFormat : uint {
			
			RGBA8 = 4,
			RGB8 = 3,
		}
	}
}
