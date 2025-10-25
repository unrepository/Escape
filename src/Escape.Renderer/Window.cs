using Escape.Renderer.OpenGL;
using Escape.Renderer.Vulkan;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Escape.Renderer {
	
	public abstract class Window : IDisposable {
		
		public string Title {
			get => Base.Title;
			set => Base.Title = value;
		}

		public uint Width {
			get => (uint) Base.FramebufferSize.X;
			set => Base.Size = new Vector2D<int>((int) value, Base.Size.Y);
		}
		
		public uint Height {
			get => (uint) Base.FramebufferSize.Y;
			set => Base.Size = new Vector2D<int>(Base.Size.X, (int) value);
		}

		public Vector2D<int> Size => Base.FramebufferSize;
		
		public IPlatform Platform { get; }
		public Framebuffer Framebuffer;
		
		public IWindow Base { get; protected set; }
		public IInputContext? Input { get; protected set; }
		
		public bool IsInitialized { get; set; }
		public double FrameDeltaTime { get; protected set; }

		/*public OrderedDictionary<QueuePriority, List<Action<double>>> RenderQueues { get; } = new() {
			{ QueuePriority.Lowest, [] },
			{ QueuePriority.Low, [] },
			{ QueuePriority.Normal, [] },
			{ QueuePriority.High, [] },
			{ QueuePriority.Highest, [] }
		};*/

		public Window(IPlatform platform, WindowOptions? options = null) {
			Platform = platform;
		}

		public abstract void Initialize(RenderQueue queue);

		public void Close() => Base.Close();
		public abstract void Dispose();

		public static Window Create(IPlatform platform, WindowOptions? options = null) {
			return platform switch {
				GLPlatform glPlatform => new GLWindow(glPlatform, options),
				VkPlatform vkPlatform => new VkWindow(vkPlatform, options),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
	}
}
