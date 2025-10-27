using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Escape.Core.Components;
using Escape.Renderer;
using Escape.UnitTypes;
using NLog;

namespace Escape.Core {
	
	public static class WorldExtensions {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
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
		
		public static Entity Instantiate(this World world, World source, Entity? parent = null) {
			var allQuery = new QueryDescription().WithNone<Empty>();
			var rootEntity = source.GetRootEntity();

			var cloneMap = new Dictionary<Entity, Entity>();
			
			// copy components
			source.Query(allQuery, sourceEntity => {
				var clonedEntity = world.Create();
				cloneMap[sourceEntity] = clonedEntity;
				
				foreach(var component in sourceEntity.GetAllComponents()) {
					if(component is null or Child or Parent) continue;
					_logger.Trace("World instantiate: Copying component {ComponentType}", component.GetType());
					
					clonedEntity.Add(component);
				}
			});
			
			// recreate tree
			if(parent is not null) {
				cloneMap[rootEntity].MakeChildOf(parent);
			} else {
				cloneMap[rootEntity].MakeChildOf(world.GetRootEntity());
			}
			
			source.Query(allQuery, sourceEntity => {
				if(sourceEntity == rootEntity) return;
				var clonedEntity = cloneMap[sourceEntity];
				
				if(sourceEntity.HasParent()) {
					clonedEntity.MakeChildOf(cloneMap[sourceEntity.GetParent()!.Value]);
				}

				foreach(var child in sourceEntity.GetChildren()) {
					cloneMap[child].MakeChildOf(clonedEntity);
				}
			});

			return cloneMap[rootEntity];
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
