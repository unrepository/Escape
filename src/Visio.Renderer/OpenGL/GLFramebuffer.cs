using System.Diagnostics;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Visio.Renderer.OpenGL {
	
	public class GLFramebuffer : Framebuffer {

		private readonly GLPlatform _platform;
		
		public GLFramebuffer(GLPlatform platform, RenderQueue queue, Vector2D<uint> size, GLTexture? baseTexture = null)
			: base(platform, queue, size)
		{
			_platform = platform;
			Handle = platform.API.GenFramebuffer();
		}
		
		public override void Bind() {
			if(Handle == 0) Create();
			_platform.API.BindFramebuffer(FramebufferTarget.Framebuffer, (uint) Handle);
			_platform.API.Viewport(0, 0, Size.X, Size.Y);
		}
		
		public override void Unbind() {
			_platform.API.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}

		public override void Create() {
			Bind();

			if(TextureAttachments.Count == 0) {
				AttachTexture(new GLTexture(_platform, Size, Texture.TextureFilter.Nearest, Texture.TextureWrapMode.ClampToBorder, Texture.TextureFormat.RGBA8));
			}
			
			// check completeness
			if(_platform.API.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete) {
				throw new PlatformException("Framebuffer is incomplete");
			}
			
			Unbind();
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

		public override void CreateAttachment(AttachmentType type) {
			throw new NotImplementedException();
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
			
			_platform.API.DeleteFramebuffer((uint) Handle);
			
			foreach(var texture in TextureAttachments) {
				texture.Dispose();
			}

			Handle = 0;
		}
	}
}
