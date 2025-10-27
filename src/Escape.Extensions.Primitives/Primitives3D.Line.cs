using System.Numerics;
using Arch.Core;
using Escape.Core.Components;
using Escape.Renderer;

namespace Escape.Extensions.Primitives {
	
	public static partial class Primitives3D {

		public static Entity Create3DLine(this World world, Material material, Vector3 A, Vector3 B, float width = 1.0f) {
			var AB = A - B;
			var u = Vector3.Normalize(AB);
			var v = Vector3.Normalize(Vector3.Cross(
				u,
				Math.Abs(Vector3.Dot(u, Vector3.UnitY)) < 0.99999f 
					? Vector3.UnitY 
					: Vector3.UnitX
			));

			var offset = v * (width / 2.0f);

			return world.Create3DPrimitive(
				material,
				[
					new() { Position = A + offset },
					new() { Position = A - offset },
					new() { Position = B + offset },
					new() { Position = B - offset }
				],
				[
					0, 1, 2, 2, 1, 3
				],
				new Transform3D(Vector3.Zero, null, null)
			);
		}
	}
}
