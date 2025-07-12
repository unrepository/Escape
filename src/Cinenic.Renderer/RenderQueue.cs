using System.Numerics;

namespace Cinenic.Renderer {
	
	public abstract class RenderQueue : IDisposable {
		
		public IPlatform Platform { get; }
		public Family Type { get; }
		
		public Vector4 Viewport { get; set; }
		public Vector4 Scissor { get; set; }
		
		public RenderQueue(
			IPlatform platform, Family family,
			Window window
		) : this(platform, family) {
			Viewport = new Vector4(0, 0, window.Width, window.Height);
			Scissor = new Vector4(0, 0, window.Width, window.Height);
		}

		public RenderQueue(
			IPlatform platform, Family family,
			Vector4 viewport, Vector4 scissor
		) : this(platform, family) {
			Viewport = viewport;
			Scissor = scissor;
		}

		public RenderQueue(IPlatform platform, Family family) {
			Platform = platform;
			Type = family;
		}

		public abstract void Initialize();
		public abstract void Dispose();

		// public static RenderQueue Create(IPlatform platform, Family family) {
		// 	return platform switch {
		// 		_ => throw new NotImplementedException() // Platform_Impl
		// 	};
		// }

		public enum Family {
			
			Graphics,
			Compute,
		}
	}
}
