using Escape.Renderer;

namespace Escape {
	
	public interface IUpdater {
		
		public string Id { get; }

		public void Update(TimeSpan delta);
	}
}
