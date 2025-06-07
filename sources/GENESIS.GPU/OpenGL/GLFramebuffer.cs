using System.Diagnostics;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace GENESIS.GPU.OpenGL {
	
	public class GLFramebuffer : IFramebuffer {

		public uint Handle { get; private set; }
		public Vector2D<uint> Size { get; private set; }

		private readonly GLPlatform _platform;
		private List<ITexture> _textureAttachments = [];
		
		public GLFramebuffer(GLPlatform platform, Vector2D<uint> size, GLTexture? baseTexture = null) {
			_platform = platform;
			Size = size;

			Handle = platform.API.GenFramebuffer();
			Bind();
			
			// create and attach 2d texture
			baseTexture ??= new GLTexture(platform, size);
			AttachTexture(baseTexture);
			
			// check completeness
			if(platform.API.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete) {
				throw new PlatformException("Framebuffer is incomplete");
			}
			
			Unbind();
		}
		
		public void Bind() {
			Debug.Assert(Handle != 0);
			_platform.API.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
			_platform.API.Viewport(0, 0, Size.X, Size.Y);
		}
		
		public void Unbind() {
			_platform.API.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}

		public void AttachTexture(ITexture texture) {
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

		public void Resize(Vector2D<int> size) {
			throw new NotImplementedException();
		}
		
		public void Dispose() {
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
