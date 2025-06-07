using System.Diagnostics;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace GENESIS.GPU.OpenGL {
	
	public class GLFramebuffer : Framebuffer {

		private readonly GLPlatform _platform;
		private List<Texture> _textureAttachments = [];
		
		public GLFramebuffer(GLPlatform platform, Vector2D<uint> size, GLTexture? baseTexture = null) : base(size) {
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
			
			Debug.Assert(_textureAttachments.Count < 32);

			Bind();
			_platform.API.FramebufferTexture2D(
				FramebufferTarget.Framebuffer,
				FramebufferAttachment.ColorAttachment0 + _textureAttachments.Count,
				TextureTarget.Texture2D,
				texture.Handle,
				0
			);
			
			_textureAttachments.Add(texture);
		}

		public override void Resize(Vector2D<int> size) {
			throw new NotImplementedException();
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
			Debug.Assert(Handle != 0);
			
			_platform.API.DeleteFramebuffer(Handle);
			
			foreach(var texture in _textureAttachments) {
				texture.Dispose();
			}

			Handle = 0;
		}
	}
}
