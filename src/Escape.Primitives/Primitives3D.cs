using Arch.Core;
using Escape.Core;
using Escape.Core.Components;
using Escape.Renderer;

namespace Escape.Primitives {
	
	public static partial class Primitives3D {

		private static Entity Create3DPrimitive(
			this World world,
			Material material,
			Vertex[] vertices,
			uint[] indices,
			Transform3D t3d
		) {
			var model = new Model {
				Meshes = [
					new Mesh {
						Vertices = vertices,
						Indices = indices,
						Material = material
					}
				]
			};

			return world.Create3DObject(new RenderableObject(model), t3d);
		}
	}
}
