using Arch.Core;
using Arch.Core.Extensions;
using Escape.Core.Components;
using Escape.Core.Scripting.Systems;
using Escape.Core.Systems;
using Escape.Renderer;

namespace Escape.Core {
	
	public class WorldUpdater : IUpdater, IDisposable {

		public string Id { get; }
		public World World { get; }

		public CameraUpdateSystem CameraSystem { get; }
		public HierarchyUpdateSystem HierarchySystem { get; }
		public RelationshipTracker RelationshipTracker { get; }
		public ScriptTracker ScriptTracker { get; }
		public StateTracker StateTracker { get; }
		
		public WorldUpdater(IPlatform platform, string id, World world) {
			Id = id;
			World = world;

			try {
				world.GetRootEntity();
			} catch(InvalidDataException) {
				world.CreateRootEntity();
			}

			RelationshipTracker = new RelationshipTracker(world);
			ScriptTracker = new ScriptTracker(platform, world);
			StateTracker = new StateTracker(world);
			
			CameraSystem = new CameraUpdateSystem(world);
			HierarchySystem = new HierarchyUpdateSystem(world) {
				DebugPrintHierarchy = false
			};
		}
		
		public virtual void Update(TimeSpan delta) {
			CameraSystem.Update(delta);
			HierarchySystem.Update(delta);
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			
			CameraSystem.Dispose();
			HierarchySystem.Dispose();
			RelationshipTracker.Dispose();
			ScriptTracker.Dispose();
			StateTracker.Dispose();
		}
	}
}
