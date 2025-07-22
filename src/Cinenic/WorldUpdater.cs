using EcsWorld = Arch.Core.World;

namespace Cinenic {
	
	public class WorldUpdater : IUpdater {

		public string Id { get; }
		public EcsWorld World { get; set; }
		
		public WorldUpdater(string id, EcsWorld world) {
			Id = id;
			World = world;
		}
		
		public virtual void Update(TimeSpan delta) {
			//World.Progress((float) delta.TotalSeconds);
		}
	}
}
