using Escape.Renderer;

namespace Escape.Core {
	
	public interface IUpdater {
		
		public string Id { get; }

		public void Update(TimeSpan delta);
	}
}
