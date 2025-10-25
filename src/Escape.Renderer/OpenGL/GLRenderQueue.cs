using System.Diagnostics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

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
			
			if(RenderTarget is GLWindow.WindowFramebuffer windowFramebuffer) {
				var window = windowFramebuffer.Window.Base;
				
				window.DoUpdate();
				
				if(!window.IsClosing) window.DoEvents();
				if(window.IsClosing) {
					RenderTarget.Dispose();
					RenderTarget = null;
					
					window.IsVisible = false;
					window.Close();
					return false;
				}
				
				window.MakeCurrent();
			} else {
				_platform.API.BindFramebuffer(FramebufferTarget.DrawFramebuffer, (uint) RenderTarget.Handle);
			}
			
			var viewportWidth = Viewport.Z > 0 ? (uint) Viewport.Z : RenderTarget.Size.X;
			var viewportHeight = Viewport.W > 0 ? (uint) Viewport.W : RenderTarget.Size.Y;
			
			_platform.API.Viewport(
				Viewport.X,
				Viewport.Y,
				viewportWidth,
				viewportHeight
			);

			var scissorWidth = Scissor.Z > 0 ? (uint) Scissor.Z : viewportWidth;
			var scissorHeight = Scissor.W > 0 ? (uint) Scissor.W : viewportHeight;
			
			_platform.API.Scissor(
				Scissor.X,
				Scissor.Y,
				scissorWidth,
				scissorHeight
			);
			
			_platform.API.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_platform.API.ClearColor(ClearColor);

			return true;
		}

		public override bool End() {
			if(RenderTarget is null) return false;
			
			if(RenderTarget is GLWindow.WindowFramebuffer windowFramebuffer) {
				var window = windowFramebuffer.Window.Base;
				window.SwapBuffers();
			}
			
			return true;
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
