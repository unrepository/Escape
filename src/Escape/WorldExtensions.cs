using System.Numerics;
using Arch.Core;
using Escape.Components;
using Escape.Renderer;
using Escape.UnitTypes;

namespace Escape {
	
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

		public static Entity Create3DObject(this World world, RenderableObject obj, Transform3D t3d) {
			return world.Create(obj, t3d);
		}

		public static Entity Create3DObject(this World world, RenderableObject obj, Vector3 position, Quaternion? rotation, Vector3? scale) {
			return world.Create3DObject(
				obj,
				new Transform3D(position, rotation ?? Quaternion.Identity, scale ?? Vector3.One)
			);
		}

		public static Entity Create3DObject(
			this World world,
			RenderableObject obj,
			Vector3 position,
			Rotation<float>? yaw, Rotation<float>? pitch, Rotation<float>? roll,
			Vector3? scale
		) {
			return world.Create3DObject(
				obj,
				new Transform3D(
					position,
					yaw ?? Rotation<float>.FromRadians(0),
					pitch ?? Rotation<float>.FromRadians(0),
					roll ?? Rotation<float>.FromRadians(0),
					scale ?? Vector3.One
				)
			);
		}
	}
}
