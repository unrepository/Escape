using System.Numerics;
using Silk.NET.Maths;

namespace Cinenic.Renderer {
	
	public abstract class RenderQueue : IDisposable {
		
		public IPlatform Platform { get; }
		public Family Type { get; }
		public Format ColorFormat { get; }
		
		public Vector4D<int> Viewport { get; set; }
		public Vector4D<int> Scissor { get; set; }
		
		public Framebuffer RenderTarget { get; set; }
		
		// public RenderQueue(
		// 	IPlatform platform, Family family, Format format,
		// 	Window window
		// ) : this(platform, family, format) {
		// 	Viewport = new Vector4D<int>(0, 0, (int) window.Width, (int) window.Height);
		// 	Scissor = new Vector4D<int>(0, 0, (int) window.Width, (int) window.Height);
		// 	RenderTarget = window.Framebuffer;
		// }
		//
		// public RenderQueue(
		// 	IPlatform platform, Family family, Format format,
		// 	Vector4D<int> viewport, Vector4D<int> scissor,
		// 	Framebuffer renderTarget
		// ) : this(platform, family, format) {
		// 	Viewport = viewport;
		// 	Scissor = scissor;
		// 	RenderTarget = renderTarget;
		// }

		public RenderQueue(IPlatform platform, Family family, Format format) {
			Platform = platform;
			Type = family;
			ColorFormat = format;
		}

		public abstract void Initialize();

		public abstract void Begin(Framebuffer renderTarget);
		public abstract void End(Framebuffer renderTarget);
		
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
