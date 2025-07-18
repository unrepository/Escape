using System.Numerics;

namespace Cinenic.World.Components {

	public struct Transform3D(Vector3 position, Quaternion rotation, Vector3 scale) {

		public Vector3 Position = position;
		public Quaternion Rotation = rotation;
		public Vector3 Scale = scale;
	}
}
