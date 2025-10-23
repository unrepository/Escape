using System.Collections.ObjectModel;
using System.Numerics;
using Escape.Renderer.Shader;
using Silk.NET.Maths;

namespace Escape.Renderer {
	
	public abstract class RenderQueue : IDisposable {
		
		public IPlatform Platform { get; }
		public Family Type { get; }
		public Format ColorFormat { get; }
		
		public Vector4D<int> Viewport { get; set; } = Vector4D<int>.Zero;
		public Vector4D<int> Scissor { get; set; } = Vector4D<int>.Zero;
		
		public RenderPipeline? Pipeline { get; internal set; }
		public Framebuffer? RenderTarget;
		
		protected SortedDictionary<int, List<IRenderer>> Queue { get; set; } = [];
		
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
			foreach(var (priority, renderers) in Queue) {
				foreach(var renderer in renderers) {
					renderer.Render(this, delta);
				}
			}
		}
		
		public abstract bool End();

		public void Enqueue(IRenderer renderer) {
			if(Queue.TryGetValue(renderer.Priority, out var renderers)) {
				renderers.Add(renderer);
				return;
			}

			Queue[renderer.Priority] = [ renderer ];
		}

		public bool Dequeue(IRenderer renderer) {
			if(!Queue.TryGetValue(renderer.Priority, out var renderers)) {
				return false;
			}

			return renderers.Remove(renderer);
		}

		// public int Dequeue(IRenderer renderer) {
		// 	int dequeued = 0;
		//
		// 	foreach(var renderers in Queue.Values) {
		// 		dequeued += renderers.RemoveAll(r => r == renderer);
		// 	}
		//
		// 	return dequeued;
		// }

		public ReadOnlyDictionary<int, List<IRenderer>> GetQueue() => new(Queue);
		public void ClearQueue() => Queue.Clear();
		
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
