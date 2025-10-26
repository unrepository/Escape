using Arch.Core;
using Escape.Systems;

namespace Escape {
	
	public class WorldUpdater : IUpdater {

		public string Id { get; }
		public World World { get; }

		public CameraUpdateSystem CameraSystem { get; }
		public HierarchyUpdateSystem HierarchySystem { get; }
		public RelationshipTracker RelationshipTracker { get; }
		
		public WorldUpdater(string id, World world) {
			Id = id;
			World = world;

			CameraSystem = new CameraUpdateSystem(world);
			HierarchySystem = new HierarchyUpdateSystem(world) {
				DebugPrintHierarchy = false
			};

			RelationshipTracker = new RelationshipTracker(world);
		}
		
		public virtual void Update(TimeSpan delta) {
			CameraSystem.Update(delta);
			HierarchySystem.Update(delta);
		}
	}
}
