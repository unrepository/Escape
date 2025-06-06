using System.Diagnostics;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace GENESIS.GPU.OpenGL {
	
	public class GLWindow : Window {

		public override string Title {
			get => Base.Title;
			set => Base.Title = value;
		}

		public override uint Width {
			get => (uint) Base.FramebufferSize.X;
			set => Base.Size = new Vector2D<int>((int) value, Base.Size.Y);
		}
		
		public override uint Height {
			get => (uint) Base.FramebufferSize.Y;
			set => Base.Size = new Vector2D<int>(Base.Size.X, (int) value);
		}

		private readonly GLPlatform _platform;

		public GLWindow(GLPlatform platform, WindowOptions? options = null) {
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
				_platform.API.ClearColor(0, 0, 0, 0);

				foreach(var queues in RenderQueues.Values) {
					foreach(var queue in queues) {
						queue(delta);
					}
				}
			};
			
			Base.Initialize();
		}

		public override void Initialize() {
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

			if(frameProvider is not null) RenderQueues[QueuePriority.Normal].Add(frameProvider);
			Base.DoRender();
			if(frameProvider is not null) RenderQueues[QueuePriority.Normal].Remove(frameProvider);

			return FrameDeltaTime;
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
			Base.Dispose();
		}
	}
}
