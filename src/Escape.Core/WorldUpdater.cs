using Arch.Core;
using Escape.Core.Scripting.Systems;
using Escape.Core.Systems;

namespace Escape.Core {
	
	public class WorldUpdater : IUpdater {

		public string Id { get; }
		public World World { get; }

		public CameraUpdateSystem CameraSystem { get; }
		public HierarchyUpdateSystem HierarchySystem { get; }
		public RelationshipTracker RelationshipTracker { get; }
		public ScriptTracker ScriptTracker { get; }
		
		public WorldUpdater(string id, World world) {
			Id = id;
			World = world;

			CameraSystem = new CameraUpdateSystem(world);
			HierarchySystem = new HierarchyUpdateSystem(world) {
				DebugPrintHierarchy = false
			};

			RelationshipTracker = new RelationshipTracker(world);
			ScriptTracker = new ScriptTracker(world);
		}
		
		public virtual void Update(TimeSpan delta) {
			CameraSystem.Update(delta);
			HierarchySystem.Update(delta);
		}
	}
}
