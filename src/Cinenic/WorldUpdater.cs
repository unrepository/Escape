using Arch.Core;
using Cinenic.Systems;

namespace Cinenic {
	
	public class WorldUpdater : IUpdater {

		public string Id { get; }
		public World World { get; set; }

		private WorldUpdateSystem _primarySystem;
		private HierarchyUpdateSystem _hierarchySystem;
		
		public WorldUpdater(string id, World world) {
			Id = id;
			World = world;

			_primarySystem = new WorldUpdateSystem(world);
			_hierarchySystem = new HierarchyUpdateSystem(world) {
				DebugPrintHierarchy = false
			};
		}
		
		public virtual void Update(TimeSpan delta) {
			_primarySystem.Update(delta);
			_hierarchySystem.Update(delta);
		}
	}
}
