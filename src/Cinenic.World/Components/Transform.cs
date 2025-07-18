using System.Numerics;
using Cinenic.Extensions.CSharp;

namespace Cinenic.World.Components {

	// TODO should we change euler rotations to RotationF from UnitTypes?
	public struct Transform3D(Vector3 position, Quaternion rotation, Vector3 scale) {

		public Vector3 Position = position;
		public Quaternion Rotation = rotation;
		public Vector3 Scale = scale;

		public Vector3 RotationRad {
			get => new Vector3(PitchRad, YawRad, RollRad);
			set {
				Rotation = Quaternion.CreateFromYawPitchRoll(
					value.Y,
					value.X,
					value.Z
				);
			}
		}

		public float YawRad {
			get => Rotation.GetYaw();
			set => Rotation.SetYaw(value);
		}
		
		public float PitchRad {
			get => Rotation.GetPitch();
			set => Rotation.SetPitch(value);
		}
		
		public float RollRad {
			get => Rotation.GetRoll();
			set => Rotation.SetRoll(value);
		}
		
		public Vector3 RotationDeg {
			get => new Vector3(PitchDeg, YawDeg, RollDeg);
			set {
				Rotation = Quaternion.CreateFromYawPitchRoll(
					value.Y.ToRadians(),
					value.X.ToRadians(),
					value.Z.ToRadians()
				);
			}
		}
		
		public float YawDeg {
			get => Rotation.GetYaw().ToDegrees();
			set => Rotation.SetYaw(value.ToRadians());
		}
		
		public float PitchDeg {
			get => Rotation.GetPitch().ToDegrees();
			set => Rotation.SetPitch(value.ToRadians());
		}
		
		public float RollDeg {
			get => Rotation.GetRoll().ToDegrees();
			set => Rotation.SetRoll(value.ToRadians());
		}

		public void LookAt(Vector3 target) {
			var forward = Vector3.Normalize(target - position);
			
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
