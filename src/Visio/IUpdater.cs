using Visio.Renderer;

namespace Visio {
	
	public interface IUpdater {
		
		public string Id { get; }

		public void Update(TimeSpan delta);
	}
}
