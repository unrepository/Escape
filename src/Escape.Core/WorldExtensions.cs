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

			throw new InvalidDataException("World has no root entity");

			//var entity = world.Create();
			//_rootEntities[world.Id] = entity;
			//return entity;
		}

		public static void SetRootEntity(this World world, Entity root) {
			_rootEntities[world.Id] = root;
		}

		public static IEnumerable<Entity> GetEntities(this World world) {
			/*var query = world.Query(new QueryDescription().WithNone<Empty>());

			while(query.GetChunkIterator().GetEnumerator().MoveNext()) {
				var chunk = query.GetChunkIterator().GetEnumerator().Current;
				if(chunk.Count <= 0) continue;
				
				foreach(var entity in chunk.Entities) {
					yield return entity;
				}
			}*/

			var query = new QueryDescription().WithNone<Empty>();
			var entities = new List<Entity>();

			world.Query(query, entity => {
				entities.Add(entity);
			});

			return entities;
		}
		
		public static Entity Instantiate(this World world, World source, Entity? parent = null) {
			var rootEntity = source.GetRootEntity();

			var cloneMap = new Dictionary<Entity, Entity>();
			
			// copy components
			foreach(var sourceEntity in source.GetEntities()) {
				var clonedEntity = world.Create();
				cloneMap[sourceEntity] = clonedEntity;
				
				foreach(var component in sourceEntity.GetAllComponents()) {
					if(component is null or Child or Parent) continue;
					_logger.Trace("World instantiate: Copying component {ComponentType}", component.GetType());
					
					clonedEntity.Add(component);
				}
			}
			
			// recreate tree
			if(parent is not null) {
				cloneMap[rootEntity].MakeChildOf(parent);
			} else {
				cloneMap[rootEntity].MakeChildOf(world.GetRootEntity());
			}

			foreach(var sourceEntity in source.GetEntities()) {
				if(sourceEntity == rootEntity) continue;
				var clonedEntity = cloneMap[sourceEntity];
				
				if(sourceEntity.HasParent()) {
					clonedEntity.MakeChildOf(cloneMap[sourceEntity.GetParent()!.Value]);
				}

				foreach(var child in sourceEntity.GetChildren()) {
					cloneMap[child].MakeChildOf(clonedEntity);
				}
			}

			return cloneMap[rootEntity];
		}

		public static Entity Create3DObject(this World world, Renderable obj, Transform3D t3d) {
			return world.Create(obj, t3d);
		}

		public static Entity Create3DObject(this World world, Renderable obj, Vector3 position, Quaternion? rotation, Vector3? scale) {
			return world.Create3DObject(
				obj,
				new Transform3D(position, rotation ?? Quaternion.Identity, scale ?? Vector3.One)
			);
		}

		public static Entity Create3DObject(
			this World world,
			Renderable obj,
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
