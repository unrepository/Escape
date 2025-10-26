using Arch.Core;
using Escape.Scripting.Systems;
using Escape.Systems;

namespace Escape {
	
	public class WorldUpdater : IUpdater, IDisposable {

		public string Id { get; }
		public World World { get; }

		private readonly WorldUpdateSystem _primarySystem;
		private readonly HierarchyUpdateSystem _hierarchySystem;
		private readonly RelationshipTracker _relationshipTracker;
		private readonly ScriptTracker _scriptTracker;
		
		public WorldUpdater(string id, World world) {
			Id = id;
			World = world;

			_primarySystem = new WorldUpdateSystem(world);
			
			_hierarchySystem = new HierarchyUpdateSystem(world) {
				DebugPrintHierarchy = false
			};

			_relationshipTracker = new RelationshipTracker(world);
			_scriptTracker = new ScriptTracker(world);
		}
		
		public virtual void Update(TimeSpan delta) {
			_primarySystem.Update(delta);
			_hierarchySystem.Update(delta);
		}

		public void Dispose() {
			GC.SuppressFinalize(this);

			_relationshipTracker.Active = false;
			_scriptTracker.Active = false;
		}
	}
}
