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

		protected SortedDictionary<int, List<Action<double>>> RenderQueues { get; } = [];
		
		public Window(IPlatform platform, WindowOptions? options = null) {
			Platform = platform;
		}

		public abstract void Initialize(RenderQueue queue);
		public abstract double RenderFrame(Action<double>? frameProvider = null);

		public abstract void ScheduleLater(Action action);

		public void Close() => Base.Close();

		public void AddRenderQueue(int priority, Action<double> action) {
			if(!RenderQueues.ContainsKey(priority)) {
				RenderQueues[priority] = new();
			}
			
			RenderQueues[priority].Add(action);
		}

		public bool RemoveRenderQueue(int priority, int index) {
			if(!RenderQueues.TryGetValue(priority, out var queues)) return false;
			if(index >= queues.Count) return false;
			
			queues.RemoveAt(index);
			return true;
		}

		public bool RemoveRenderQueue(int priority, Action<double> action) {
			if(!RenderQueues.TryGetValue(priority, out var queues)) return false;
			return queues.Remove(action);
		}

		public void ClearRenderQueues() => RenderQueues.Clear();
		public void ClearRenderQueues(int priority) {
			if(RenderQueues.TryGetValue(priority, out var queues)) queues.Clear();
		}

		public int RemoveRenderQueue(Action<double> action) {
			int removed = 0;
			
			foreach(var queues in RenderQueues.Values) {
				if(queues.Remove(action)) removed++;
			}

			return removed;
		}
		
		public abstract void Dispose();

		public static Window Create(IPlatform platform, WindowOptions? options = null) {
			return platform switch {
				GLPlatform glPlatform => new GLWindow(glPlatform, options),
				VkPlatform vkPlatform => new VkWindow(vkPlatform, options),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
		
		public enum QueuePriority {
			
			Lowest,
			Low,
			Normal,
			High,
			Highest
		}
	}
}
