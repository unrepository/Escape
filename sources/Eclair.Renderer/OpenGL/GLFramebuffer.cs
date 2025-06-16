using System.Diagnostics;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Eclair.Renderer.OpenGL {
	
	public class GLFramebuffer : Framebuffer {

		private readonly GLPlatform _platform;
		
		public GLFramebuffer(GLPlatform platform, Vector2D<uint> size, GLTexture? baseTexture = null)
			: base(platform, size)
		{
			_platform = platform;

			Handle = platform.API.GenFramebuffer();
			Bind();
			
			// create and attach 2d texture
			baseTexture ??= new GLTexture(platform, size, Texture.Filter.Nearest, Texture.WrapMode.Clamp);
			AttachTexture(baseTexture);
			
			// check completeness
			if(platform.API.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete) {
				throw new PlatformException("Framebuffer is incomplete");
			}
			
			Unbind();
		}
		
		public override void Bind() {
			Debug.Assert(Handle != 0);
			_platform.API.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
			_platform.API.Viewport(0, 0, Size.X, Size.Y);
		}
		
		public override void Unbind() {
			_platform.API.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}

		public override void AttachTexture(Texture texture) {
			if(texture is not GLTexture) {
				throw new ArgumentException("Not a GL texture", nameof(texture));
			}
			
			Debug.Assert(TextureAttachments.Count < 32);

			Bind();
			_platform.API.FramebufferTexture2D(
				FramebufferTarget.Framebuffer,
				FramebufferAttachment.ColorAttachment0 + TextureAttachments.Count,
				TextureTarget.Texture2D,
				((GLTexture) texture).Id,
				0
			);
			
			TextureAttachments.Add(texture);
		}

		public override void Resize(Vector2D<int> size) {
			throw new NotImplementedException();
		}

		public unsafe override byte[] Read(int attachment = 0, Rectangle<uint>? area = null) {
			Bind();
			_platform.API.ReadBuffer(ReadBufferMode.ColorAttachment0 + attachment);

			area ??= new Rectangle<uint>(0, 0, Size);
			byte[] pixels = new byte[area.Value.Size.X * area.Value.Size.Y * 4]; // Rgba8

			fixed(void* ptr = &pixels[0]) {
				_platform.API.ReadPixels(
					(int) area.Value.Origin.X, (int) area.Value.Origin.Y,
					area.Value.Size.X, area.Value.Size.Y,
					PixelFormat.Rgba, PixelType.UnsignedByte,
					ptr
				);
			}

			return pixels;
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			Debug.Assert(Handle != 0);
			
			_platform.API.DeleteFramebuffer(Handle);
			
			foreach(var texture in TextureAttachments) {
				texture.Dispose();
			}

			Handle = 0;
		}
	}
}
