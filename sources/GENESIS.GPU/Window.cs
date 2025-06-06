using GENESIS.GPU.OpenGL;
using Silk.NET.Windowing;

namespace GENESIS.GPU {
	
	public abstract class Window : IDisposable {
		
		public abstract string Title { get; set; }
		public abstract uint Width { get; set; }
		public abstract uint Height { get; set; }
		
		public bool IsInitialized { get; set; }
		public double FrameDeltaTime { get; protected set; }

		public OrderedDictionary<QueuePriority, List<Action<double>>> RenderQueues { get; } = new() {
			{ QueuePriority.Lowest, [] },
			{ QueuePriority.Low, [] },
			{ QueuePriority.Normal, [] },
			{ QueuePriority.High, [] },
			{ QueuePriority.Highest, [] }
		};
		
		public IWindow Base { get; protected set; }

		public abstract void Initialize();
		public abstract double RenderFrame(Action<double>? frameProvider = null);

		public abstract void Dispose();

		public static Window Create(GLPlatform platform, WindowOptions? options = null)
			=> new GLWindow(platform, options);

		public enum QueuePriority {
			
			Lowest,
			Low,
			Normal,
			High,
			Highest
		}
	}
}
