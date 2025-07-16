using System.Numerics;

namespace Cinenic.World.Components {

	public record struct Transform3D(Vector3 Position, Quaternion Rotation, Vector3 Scale);
}
