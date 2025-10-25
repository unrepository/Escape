using System.Numerics;
using Escape.Renderer;

namespace Escape.Primitives {
	
	public class Line3D : Primitive {

		public Line3D(Vector3 startPosition, Vector3 endPosition, float width = 1.0f) : base(Vector3.Zero) {
			var A = startPosition;
			var B = endPosition;
			
			var AB = A - B;
			var u = Vector3.Normalize(AB);
			var v = Vector3.Normalize(Vector3.Cross(
				u,
				Math.Abs(Vector3.Dot(u, Vector3.UnitY)) < 0.99999f 
					? Vector3.UnitY 
					: Vector3.UnitX
			));

			var offset = v * (width / 2.0f);
			
			Vertices = [
				new() { Position = A + offset },
				new() { Position = A - offset },
				new() { Position = B + offset },
				new() { Position = B - offset }
			];

			Indices = [
				0, 1, 2, 2, 1, 3
			];
		}
	}
}
