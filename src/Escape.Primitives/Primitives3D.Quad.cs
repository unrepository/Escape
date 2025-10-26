using System.Numerics;
using Arch.Core;
using Escape.Components;
using Escape.Renderer;

namespace Escape.Primitives {
	
	public static partial class Primitives3D {

		public static readonly Model UnitQuadXZ = new Model {
			Meshes = [
				new Mesh {
					Vertices = [
						new() { Position = { X = 0, Z = 0 } },
						new() { Position = { X = 0, Z = 1 } },
						new() { Position = { X = 1, Z = 1 } },
						new() { Position = { X = 1, Z = 0 } },
					],
					Indices = [
						0, 1, 2,
						2, 1, 3
					]
				}
			]
		};
		
		public static readonly Model UnitQuadXY = new Model {
			Meshes = [
				new Mesh {
					Vertices = [
						new() { Position = { X = 0, Y = 0 } },
						new() { Position = { X = 0, Y = 1 } },
						new() { Position = { X = 1, Y = 1 } },
						new() { Position = { X = 1, Y = 0 } },
					],
					Indices = [
						0, 1, 2,
						2, 1, 3
					]
				}
			]
		};
		
		public static readonly Model UnitQuadYZ = new Model {
			Meshes = [
				new Mesh {
					Vertices = [
						new() { Position = { Y = 0, Z = 0 } },
						new() { Position = { Y = 0, Z = 1 } },
						new() { Position = { Y = 1, Z = 1 } },
						new() { Position = { Y = 1, Z = 0 } },
					],
					Indices = [
						0, 1, 2,
						2, 1, 3
					]
				}
			]
		};

		public static Entity Create3DQuadXZ(this World world, Material material, Vector3 position, float width, float depth) {
			return world.Create3DPrimitive(
				material,
				UnitQuadXZ.Meshes[0].Vertices,
				UnitQuadXZ.Meshes[0].Indices,
				new Transform3D(position, null, new Vector3(width, 1, depth))
			);
		}
		
		public static Entity Create3DQuadXY(this World world, Material material, Vector3 position, float width, float height) {
			return world.Create3DPrimitive(
				material,
				UnitQuadXY.Meshes[0].Vertices,
				UnitQuadXY.Meshes[0].Indices,
				new Transform3D(position, null, new Vector3(width, height, 1))
			);
		}
		
		public static Entity Create3DQuadYZ(this World world, Material material, Vector3 position, float height, float depth) {
			return world.Create3DPrimitive(
				material,
				UnitQuadYZ.Meshes[0].Vertices,
				UnitQuadYZ.Meshes[0].Indices,
				new Transform3D(position, null, new Vector3(1, height, depth))
			);
		}

		public static Entity Create3DQuad(this World world, Material material, Vector3 A, Vector3 B) {
			var D = B - A;
			
			var vertices = new Vertex[] {
				new() { Position = { X = 0,   Y = 0,   Z = 0   } },
				new() { Position = { X = D.X, Y = 0,   Z = 0   } },
				new() { Position = { X = D.X, Y = D.Y, Z = D.Z } },
				new() { Position = { X = 0,   Y = D.Y, Z = D.Z } },
			};
			
			var indices = new uint[] {
				0, 1, 2,
				2, 3, 0
			};
			
			return world.Create3DPrimitive(
				material,
				vertices,
				indices,
				new Transform3D(A, null, Vector3.One)
			);
		}
	}
}
