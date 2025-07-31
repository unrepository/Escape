namespace Visio.Renderer {
	
	public interface IRenderer {

		public string Id { get; }
		public int Priority { get; init; }
		
		public void Render(RenderQueue queue, TimeSpan delta);
	}
	
	public class WindowRenderer : IRenderer {

		public string Id { get; }
		public int Priority { get; init; }
		
		public Window Window { get; }

		public WindowRenderer(Window window) {
			Id = window.GetHashCode().ToString();
			Window = window;
		}

		public void Render(RenderQueue queue, TimeSpan delta) { } // TODO?

		public static implicit operator WindowRenderer(Window window) {
			return new WindowRenderer(window);
		}
	}
}
