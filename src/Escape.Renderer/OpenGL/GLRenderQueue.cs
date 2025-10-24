using System.Diagnostics;
using Silk.NET.OpenGL;

namespace Escape.Renderer.OpenGL {
	
	public class GLRenderQueue : RenderQueue {

		private readonly GLPlatform _platform;

		public GLRenderQueue(GLPlatform platform, Family family, Format format)
			: base(platform, family, format)
		{
			_platform = platform;
		}
		
		public override void Initialize() { }

		public override bool Begin() {
			if(RenderTarget is null) return false;
			
			Debug.Assert(RenderTarget is GLFramebuffer);
			
			_platform.API.BindFramebuffer(FramebufferTarget.DrawFramebuffer, (uint) RenderTarget.Handle);
			_platform.API.Viewport(Viewport.X, Viewport.Y, (uint) Viewport.Z, (uint) Viewport.W);
			_platform.API.Scissor(Scissor.X, Scissor.Y, (uint) Scissor.Z, (uint) Scissor.W);
			
			_platform.API.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_platform.API.ClearColor(ClearColor);

			return true;
		}

		public override bool End() {
			
			return true;
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
