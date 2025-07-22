using Arch.Core;
using Cinenic.Systems;

namespace Cinenic {
	
	public class WorldUpdater : IUpdater {

		public string Id { get; }
		public World World { get; set; }

		private MainUpdateSystem _primarySystem;
		
		public WorldUpdater(string id, World world) {
			Id = id;
			World = world;

			_primarySystem = new MainUpdateSystem(world);
		}
		
		public virtual void Update(TimeSpan delta) {
			_primarySystem.Update(delta);
		}
	}
}
