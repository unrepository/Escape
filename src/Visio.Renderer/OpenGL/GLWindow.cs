using System.Diagnostics;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Visio.Renderer.OpenGL {
	
	public class GLWindow : Window {

		private readonly GLPlatform _platform;
		private readonly List<Action> _scheduledActions = [];

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
					MajorVersion = 4,
					MinorVersion = 5
				}
			};
			windowOptions.SharedContext = GLPlatform._sharedContext;
			windowOptions.ShouldSwapAutomatically = true;
			windowOptions.TransparentFramebuffer = true;

			Base = Silk.NET.Windowing.Window.Create(windowOptions);
			
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
			
			Base.Render += delta => {
				FrameDeltaTime = delta;

				if(!Base.IsVisible) return;
				Base.MakeCurrent();
				
				_platform.API.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
				_platform.API.ClearColor(0, 0, 0, 1);

				foreach(var queues in RenderQueues.Values) {
					foreach(var queue in queues.ToArray()) {
						queue(delta);
					}
				}
			};
			
			Base.Initialize();
		}

		public override void Initialize(RenderQueue queue) {
			Debug.Assert(!IsInitialized);
			
			Base.MakeCurrent();
			_platform.API.Viewport(0, 0, Width, Height);

			Base.IsVisible = true;
			IsInitialized = true;
		}

		public override double RenderFrame(Action<double>? frameProvider = null) {
			Base.DoEvents();
			if(!Base.IsClosing) Base.DoUpdate();
			if(Base.IsClosing) return -1;

			if(frameProvider is not null) AddRenderQueue(0, frameProvider);
			Base.DoRender();
			if(frameProvider is not null) RemoveRenderQueue(0, frameProvider);

			foreach(var action in _scheduledActions) {
				action();
			}
			
			_scheduledActions.Clear();

			return FrameDeltaTime;
		}

		public override void ScheduleLater(Action action) {
			_scheduledActions.Add(action);
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			Base.Dispose();
		}
	}
}
