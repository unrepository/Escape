using System.Numerics;
using Arch.Core;
using Escape.Components;
using Escape.Renderer;

namespace Escape.Primitives {
	
	public class PrimitiveManager {
		
		public World World { get; }

		private readonly Dictionary<Primitive, Entity> _entities = [];

		public PrimitiveManager(World world) {
			World = world;
		}

		public Entity Add(Primitive primitive, Material material, Entity? parent = null) {
			var e = World.Create(
				new Transform3D(primitive.Position, Quaternion.Zero, Vector3.One),
				new RenderableObject(new Model { Meshes = [primitive] })
			);
			
			if(parent is not null) e.MakeChildOf(parent);

			_entities[primitive] = e;
			return e;
		}

		public Entity Add(Primitive primitive, Color? color = null, Entity? parent = null) {
			return Add(
				primitive,
				new Material {
					AlbedoColor = color.GetValueOrDefault(Color.White),
					Roughness = 1.0f,
					Metallic = 0.0f
				},
				parent
			);
		}

		public bool Remove(Primitive primitive) {
			if(!_entities.TryGetValue(primitive, out var e)) return false;
			World.Destroy(e);

			return true;
		}
	}
}
