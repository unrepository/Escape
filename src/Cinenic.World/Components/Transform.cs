using System.Numerics;
using Cinenic.Extensions.CSharp;
using Cinenic.UnitTypes;

namespace Cinenic.World.Components {

	// TODO should we change euler rotations to RotationF from UnitTypes?
	public struct Transform3D {

		public Vector3 Position;
		public Quaternion Rotation;
		public Vector3 Scale;
		
		public Rotation<float> Yaw {
			get => Rotation<float>.FromRadians(Rotation.GetYaw());
			set => Rotation.SetYaw(value.Radians);
		}
		
		public Rotation<float> Pitch {
			get => Rotation<float>.FromRadians(Rotation.GetPitch());
			set => Rotation.SetPitch(value.Radians);
		}
		
		public Rotation<float> Roll {
			get => Rotation<float>.FromRadians(Rotation.GetRoll());
			set => Rotation.SetRoll(value.Radians);
		}

		public Vector3 RotationDegrees {
			get => new Vector3(Pitch.Degrees, Yaw.Degrees, Roll.Degrees);
			set {
				Rotation = Quaternion.CreateFromYawPitchRoll(
					value.Y.ToRadians(),
					value.X.ToRadians(),
					value.Z.ToRadians()
				);
			}
		}
		
		public Vector3 RotationRadians {
			get => new Vector3(Pitch.Radians, Yaw.Radians, Roll.Radians);
			set {
				Rotation = Quaternion.CreateFromYawPitchRoll(
					value.Y,
					value.X,
					value.Z
				);
			}
		}

		public Transform3D() {
			Position = Vector3.Zero;
			Rotation = Quaternion.Zero;
			Scale = Vector3.One;
		}

		public Transform3D(Vector3 position, Quaternion rotation, Vector3 scale) {
			Position = position;
			Rotation = rotation;
			Scale = scale;
		}

		public void LookAt(Vector3 target) {
			var forward = Vector3.Normalize(target - Position);
			
			if(forward == Vector3.Zero) {
				Rotation = Quaternion.Identity;
			}

			var right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, forward));
			var up = Vector3.Cross(forward, right);

			var matrix = new Matrix4x4(
				right.X,   right.Y,   right.Z,   0,
				up.X,      up.Y,      up.Z,      0,
				forward.X, forward.Y, forward.Z, 0,
				0,         0,         0,         1
			);

			Rotation = Quaternion.CreateFromRotationMatrix(matrix);
		}
	}
}
