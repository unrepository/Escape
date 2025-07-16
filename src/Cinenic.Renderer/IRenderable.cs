namespace Cinenic.Renderer {
	
	public interface IRenderable {

		public string Id { get; }
		
		public void Render(RenderQueue queue, TimeSpan delta);
	}
	
	public class WindowRenderable : IRenderable {

		public string Id { get; }
		public Window Window { get; }

		public WindowRenderable(Window window) {
			Id = window.GetHashCode().ToString();
			Window = window;
		}

		public void Render(RenderQueue queue, TimeSpan delta) { } // TODO?

		public static implicit operator WindowRenderable(Window window) {
			return new WindowRenderable(window);
		}
	}
}
