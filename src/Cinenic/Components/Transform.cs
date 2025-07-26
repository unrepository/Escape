using System.Numerics;
using Arch.Core;
using Cinenic.Extensions.CSharp;
using Cinenic.UnitTypes;

namespace Cinenic.Components {

	[Component]
	public struct Transform3D {

		public Matrix4x4 LocalMatrix { get; internal set; }
		public Matrix4x4 GlobalMatrix { get; internal set; }

		public Vector3 Position {
			get;
			set {
				field = value;
				//LocalMatrix = CreateMatrix(this);
				IsDirty = true;
			}
		}

		public Vector3 GlobalPosition {
			get {
				Matrix4x4.Decompose(GlobalMatrix, out _, out _, out var translation);
				return translation;
			}
			set => throw new NotImplementedException();
		}

		public Quaternion Rotation {
			get;
			set {
				field = value;
				//LocalMatrix = CreateMatrix(this);
				IsDirty = true;
			}
		}

		public Quaternion GlobalRotation {
			get {
				Matrix4x4.Decompose(GlobalMatrix, out _, out var rotation, out _);
				return rotation;
			}
			set => throw new NotImplementedException();
		}

		public Vector3 Scale {
			get;
			set {
				field = value;
				//LocalMatrix = CreateMatrix(this);
				IsDirty = true;
			}
		}
		
		public Vector3 GlobalScale {
			get {
				Matrix4x4.Decompose(GlobalMatrix, out var scale, out _, out _);
				return scale;
			}
			set => throw new NotImplementedException();
		}

		public bool IsDirty = true;
		
		public Rotation<float> Yaw {
			get => Rotation<float>.FromRadians(Rotation.GetYaw());
			set {
				var q = Rotation;
				q.SetYaw(value.Radians);
				Rotation = q;
			}
		}
		
		public Rotation<float> Pitch {
			get => Rotation<float>.FromRadians(Rotation.GetPitch());
			set {
				var q = Rotation;
				q.SetPitch(value.Radians);
				Rotation = q;
			}
		}
		
		public Rotation<float> Roll {
			get => Rotation<float>.FromRadians(Rotation.GetRoll());
			set {
				var q = Rotation;
				q.SetRoll(value.Radians);
				Rotation = q;
			}
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
			Rotation = Quaternion.Identity;
			Scale = Vector3.One;
		}

		public Transform3D(Vector3 position, Quaternion rotation, Vector3 scale) {
			Position = position;
			Rotation = rotation;
			Scale = scale;
		}

		public void Translate(Vector3 translation) {
			Position += translation;
		}

		public void Translate(float x = 0, float y = 0, float z = 0) {
			Position += new Vector3(x, y, z);
		}

		public void Rotate(Rotation<float> yaw = default, Rotation<float> pitch = default, Rotation<float> roll = default) {
			Rotation *= Quaternion.CreateFromYawPitchRoll(
				yaw.Radians,
				pitch.Radians,
				roll.Radians
			);
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

		public override string ToString() {
			return
				$"[Position={Position.ToString("F3")} "
				+ $"Rotation=<X: {Pitch} Y: {Yaw} Z: {Roll}> "
				+ $"Scale=<X: {Scale.X:F3} Y: {Scale.Y:F3} Z: {Scale.Z:F3}/\n"
				
				+ $"/GlobalPosition={GlobalPosition.ToString("F3")} "
				+ $"GlobalRotation=<X: {GlobalRotation.GetPitch().ToDegrees():F3} Y: {GlobalRotation.GetYaw().ToDegrees():F3} Z: {GlobalRotation.GetRoll().ToDegrees():F3}> "
				+ $"GlobalScale=<X: {GlobalScale.X:F3} Y: {GlobalScale.Y:F3} Z: {GlobalScale.Z:F3}]";
		}

		public static Matrix4x4 CreateMatrix(Transform3D t3d) {
			return
				Matrix4x4.CreateScale(t3d.Scale)
				* Matrix4x4.CreateFromQuaternion(t3d.Rotation)
				* Matrix4x4.CreateTranslation(t3d.Position);
		}
	}
}
