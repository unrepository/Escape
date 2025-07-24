using Cinenic.Renderer;

namespace Cinenic.Extensions.Debugging.Providers {
	
	public abstract class DebugInfoProvider {
		
		public abstract string Title { get; }
		public bool IsOpen = false;

		public abstract void Render();
	}
}
