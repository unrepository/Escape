using EcsWorld = Flecs.NET.Core.World;

namespace Cinenic.World {
	
	public class WorldUpdater : IUpdater {

		public string Id { get; }
		public EcsWorld World { get; set; }
		
		public WorldUpdater(string id, EcsWorld world) {
			Id = id;
			World = world;
		}
		
		public void Update(TimeSpan delta) {
			World.Progress((float) delta.TotalSeconds);
		}
	}
}
