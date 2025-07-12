using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Vulkan;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Cinenic.Renderer {
	
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
		
		public IPlatform Platform { get; }
		public RenderPipeline Pipeline { get; }
		public Framebuffer Framebuffer { get; protected set; }
		
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
		
		public Window(IPlatform platform, RenderPipeline pipeline, WindowOptions? options = null) {
			Platform = platform;
			Pipeline = pipeline;
		}

		public abstract void Initialize();
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
		
		[Obsolete]
		public static Window Create(GLPlatform platform, WindowOptions? options = null)
			=> new GLWindow(platform, null, options);

		public static Window Create(IPlatform platform, RenderPipeline pipeline, WindowOptions? options = null) {
			return platform switch {
				VkPlatform vkPlatform => new VkWindow(vkPlatform, pipeline, options),
				_ => throw new NotImplementedException("Platform_Impl")
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
