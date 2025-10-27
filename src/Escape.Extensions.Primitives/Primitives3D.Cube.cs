using System.Numerics;
using Arch.Core;
using Escape.Core.Components;
using Escape.Renderer;

namespace Escape.Extensions.Primitives {
	
	public static partial class Primitives3D {

		public static readonly Model UnitCube = new Model {
			Meshes = [
				new Mesh {
					Vertices = [
						new() { Position = { X = 0, Y = 0, Z = 0 } },
						new() { Position = { X = 1, Y = 0, Z = 0 } },
						new() { Position = { X = 1, Y = 1, Z = 0 } },
						new() { Position = { X = 0, Y = 1, Z = 0 } },
						new() { Position = { X = 0, Y = 0, Z = 1 } },
						new() { Position = { X = 1, Y = 0, Z = 1 } },
						new() { Position = { X = 1, Y = 1, Z = 1 } },
						new() { Position = { X = 0, Y = 1, Z = 1 } }
					],
					Indices = [
						0, 1, 2, 0, 2, 3,
						4, 6, 5, 4, 7, 6,
						4, 0, 3, 4, 3, 7,
						1, 5, 6, 1, 6, 2,
						4, 5, 1, 4, 1, 0,
						3, 2, 6, 3, 6, 7
					]
				}
			]
		};
		
		public static Entity Create3DCube(this World world, Material material, Vector3 position, float width, float height, float depth) {
			return world.Create3DPrimitive(
				material,
				UnitCube.Meshes[0].Vertices,
				UnitCube.Meshes[0].Indices,
				new Transform3D(position, null, new Vector3(width, height, depth))
			);
		}

		// TODO fix this
		public static Entity Create3DCube(this World world, Material material, Vector3 A, Vector3 B) {
			/*var vertices = new Vertex[] {
				new() { Position = { X = A.X, Y = A.Y, Z = A.Z } },
				new() { Position = { X = B.X, Y = A.Y, Z = A.Z } },
				new() { Position = { X = B.X, Y = B.Y, Z = A.Z } },
				new() { Position = { X = A.X, Y = B.Y, Z = A.Z } },
				new() { Position = { X = A.X, Y = A.Y, Z = B.Z } },
				new() { Position = { X = B.X, Y = A.Y, Z = B.Z } },
				new() { Position = { X = B.X, Y = B.Y, Z = B.Z } },
				new() { Position = { X = A.X, Y = B.Y, Z = B.Z } }
			};*/

			var a1 = A;
			var b1 = B;

			A = Vector3.Min(a1, b1);
			B = Vector3.Max(a1, b1);
			
			var vertices = new Vertex[] {
				new() { Position = { X = 0, Y = 0, Z = 0 } },
				new() { Position = { X = B.X - A.X, Y = 0, Z = 0 } },
				new() { Position = { X = B.X - A.X, Y = B.Y - A.Y, Z = 0 } },
				new() { Position = { X = 0, Y = B.Y - A.Y, Z = 0 } },
				new() { Position = { X = 0, Y = 0, Z = B.Z - A.Z } },
				new() { Position = { X = B.X - A.X, Y = 0, Z = B.Z - A.Z } },
				new() { Position = { X = B.X - A.X, Y = B.Y - A.Y, Z = B.Z - A.Z } },
				new() { Position = { X = 0, Y = B.Y - A.Y, Z = B.Z - A.Z } }
			};
			
			var indices = UnitCube.Meshes[0].Indices;

			return world.Create3DPrimitive(
				material,
				vertices,
				indices,
				new Transform3D(A, null, Vector3.One)
			);
		}
	}
}
