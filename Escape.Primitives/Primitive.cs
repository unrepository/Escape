using System.Numerics;
using Escape.Renderer;

namespace Escape.Primitives {

	public abstract class Primitive : Mesh {

		public Vector3 Position { get; }

		public Primitive(Vector3 position) {
			Position = position;
		}
	}
}
