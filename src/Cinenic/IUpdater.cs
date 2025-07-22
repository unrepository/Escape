using Cinenic.Renderer;

namespace Cinenic {
	
	public interface IUpdater {
		
		public string Id { get; }

		public void Update(TimeSpan delta);
	}

	public class WindowUpdater : IUpdater {

		public string Id { get; }
		public Window Window { get; }

		public WindowUpdater(Window window) {
			Id = window.GetHashCode().ToString();
			Window = window;
		}
		
		public void Update(TimeSpan delta) {
			// Window.Base.DoUpdate();
			// if(!Window.Base.IsClosing) Window.Base.DoEvents();
			// if(Window.Base.IsClosing) UpdateManager.SetEnabled(this, false);
		}

		public static implicit operator WindowUpdater(Window window) {
			return new WindowUpdater(window);
		}
	}
}
