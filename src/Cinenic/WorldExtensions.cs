using Arch.Core;

namespace Cinenic {
	
	public static class WorldExtensions {

		private static Dictionary<int, Entity> _rootEntities = [];
		
		public static Entity GetRootEntity(this World world) {
			if(_rootEntities.TryGetValue(world.Id, out var root)) {
				return root;
			}

			var entity = world.Create();
			_rootEntities[world.Id] = entity;
			return entity;
		}

		public static void SetRootEntity(this World world, Entity root) {
			_rootEntities[world.Id] = root;
		}
	}
}
