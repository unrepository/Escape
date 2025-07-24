using Cinenic.Renderer;

namespace Cinenic {
	
	public interface IUpdater {
		
		public string Id { get; }

		public void Update(TimeSpan delta);
	}
}
