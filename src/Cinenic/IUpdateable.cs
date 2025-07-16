using Cinenic.Renderer;

namespace Cinenic {
	
	public interface IUpdateable {
		
		public string Id { get; }

		public void Update(TimeSpan delta);
	}

	public class WindowUpdateable : IUpdateable {

		public string Id { get; }
		public Window Window { get; }

		public WindowUpdateable(Window window) {
			Id = window.GetHashCode().ToString();
			Window = window;
		}
		
		public void Update(TimeSpan delta) {
			// Window.Base.DoUpdate();
			// if(!Window.Base.IsClosing) Window.Base.DoEvents();
			// if(Window.Base.IsClosing) UpdateManager.SetEnabled(this, false);
		}

		public static implicit operator WindowUpdateable(Window window) {
			return new WindowUpdateable(window);
		}
	}
}
