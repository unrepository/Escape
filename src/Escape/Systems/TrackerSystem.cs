using Arch.Core;

namespace Escape.Systems {
	
	public abstract class TrackerSystem : IDisposable {

		public World World { get; }
		public bool Active { get; set; }

		public TrackerSystem(World world, bool active = true) {
			World = world;
			Active = active;
		}
		
		public void Dispose() {
			GC.SuppressFinalize(this);
			Active = false;
		}
	}
}
