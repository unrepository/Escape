using System.Numerics;
using Silk.NET.Maths;

namespace Cinenic.Renderer {
	
	public abstract class RenderQueue : IDisposable {
		
		public IPlatform Platform { get; }
		public Family Type { get; }
		public Format ColorFormat { get; }
		
		public Vector4D<int> Viewport { get; set; } = Vector4D<int>.Zero;
		public Vector4D<int> Scissor { get; set; } = Vector4D<int>.Zero;
		
		public List<IRenderable> Queue { get; set; } = [];
		public Framebuffer? RenderTarget;
		
		/*public RenderQueue(
			IPlatform platform, Family family, Format format,
			Window window
		) : this(platform, family, format) {
			Viewport = new Vector4D<int>(0, 0, (int) window.Width, (int) window.Height);
			Scissor = new Vector4D<int>(0, 0, (int) window.Width, (int) window.Height);
		}
		
		public RenderQueue(
			IPlatform platform, Family family, Format format,
			Vector4D<int> viewport, Vector4D<int> scissor
		) : this(platform, family, format) {
			Viewport = viewport;
			Scissor = scissor;
		}*/

		public RenderQueue(IPlatform platform, Family family, Format format) {
			Platform = platform;
			Type = family;
			ColorFormat = format;
		}

		public abstract void Initialize();

		public abstract bool Begin();
		
		public virtual void Render(TimeSpan delta) {
			foreach(var renderable in Queue) {
				renderable.Render(this, delta); // TODO smth like painter
			}
		}
		
		public abstract bool End();
		
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

		public enum Format {
			
			R8G8B8A8Srgb
		}
	}
}
