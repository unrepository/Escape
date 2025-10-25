using System.Diagnostics;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Monitor = Silk.NET.Windowing.Monitor;

namespace Escape.Renderer.OpenGL {
	
	public class GLWindow : Window {

		private readonly GLPlatform _platform;

		public GLWindow(GLPlatform platform, WindowOptions? options = null)
			: base(platform, options)
		{
			_platform = platform;
			
			var windowOptions = options ?? WindowOptions.Default;
			windowOptions.API = new GraphicsAPI {
				API = ContextAPI.OpenGL,
				Flags = ContextFlags.ForwardCompatible,
				Profile = ContextProfile.Core,
				Version = new APIVersion {
					MajorVersion = 3,
					MinorVersion = 3
				}
			};
			windowOptions.SharedContext = GLPlatform._sharedContext;
			windowOptions.ShouldSwapAutomatically = false;
			windowOptions.TransparentFramebuffer = true;

			Base = Silk.NET.Windowing.Window.Create(windowOptions);

			// center window on main monitor
			var mainMonitorBounds = Monitor.GetMainMonitor(Base).Bounds;
			Base.Position = new Vector2D<int>(
				mainMonitorBounds.Origin.X + mainMonitorBounds.Size.X / 2 - Base.Size.X / 2,
				mainMonitorBounds.Origin.Y + mainMonitorBounds.Size.Y / 2 - Base.Size.Y / 2
			);
		}

		public override void Initialize(RenderQueue queue) {
			Debug.Assert(!IsInitialized);
			var glQueue = (GLRenderQueue) queue;
			
			Base.Load += () => {
				if(GLPlatform._sharedApi is null || GLPlatform._sharedContext is null) {
					GLPlatform._sharedApi = Base.CreateOpenGL();
					GLPlatform._sharedContext = Base.GLContext;
				}
				
				Input = Base.CreateInput();
			};

			Base.FramebufferResize += size => {
				Base.MakeCurrent();
				_platform.API.Viewport(size);
			};
			
			Base.Initialize();

			Framebuffer = new WindowFramebuffer(_platform, glQueue, this);
			Framebuffer.Create();

			Base.IsVisible = true;
			IsInitialized = true;
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			
			Framebuffer.Dispose();
			Base.Dispose();
		}

		public class WindowFramebuffer : GLFramebuffer {

			public GLWindow Window { get; }

			public WindowFramebuffer(GLPlatform platform, RenderQueue queue, GLWindow window)
				: base(platform, queue, (Vector2D<uint>) window.Size)
			{
				Window = window;

				window.Base.FramebufferResize += newSize => {
					Size = (Vector2D<uint>) newSize;
					OnResized(newSize);
				};
			}

			public override void Resize(Vector2D<int> size)
				=> throw new NotSupportedException();
		}
	}
}
