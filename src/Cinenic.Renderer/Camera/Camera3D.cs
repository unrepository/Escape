using System.Numerics;
using Cinenic.Extensions.CSharp;

namespace Cinenic.Renderer.Camera {
	
	public abstract class Camera3D : CameraBase {
		
		public static float ZNear { get; set; } = 0.01f;
		public static float ZFar { get; set; } = 1_000f;

		public Vector3 Position = Vector3.Zero;
		// public Vector3 Rotation = Vector3.Zero;
		//
		// public float Yaw {
		// 	get => Rotation.Y;
		// 	set {
		// 		Rotation.Y = value;
		// 		Direction.X = MathF.Cos(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
		// 		Direction.Z = MathF.Sin(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
		// 		Front = Vector3.Normalize(Direction);
		// 	}
		// }
		//
		// public float Pitch {
		// 	get => Rotation.X;
		// 	set {
		// 		Rotation.X = value;
		// 		Direction.X = MathF.Cos(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
		// 		Direction.Y = MathF.Sin(Rotation.X.ToRadians());
		// 		Direction.Z = MathF.Sin(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
		// 		Front = Vector3.Normalize(Direction);
		// 	}
		// }
		
		public Vector3 Target = Vector3.Zero;
		public Vector3 Up = -Vector3.UnitY;

		protected Camera3D(Framebuffer framebuffer) : base(framebuffer) { }
		protected Camera3D(int width, int height) : base(width, height) { }

		/*public void MoveUp(float amount) => Position += Up = Vector3.Multiply(ViewMatrix.PositiveY(), amount);
		public void MoveDown(float amount) => Position -= Up = Vector3.Multiply(ViewMatrix.PositiveY(), amount);
		public void MoveLeft(float amount) => Position -= Vector3.Multiply(ViewMatrix.PositiveX(), amount);
		public void MoveRight(float amount) => Position += Vector3.Multiply(ViewMatrix.PositiveX(), amount);
		public void MoveForward(float amount) => Position -= Direction = Vector3.Multiply(ViewMatrix.PositiveZ(), amount);
		public void MoveBackward(float amount) => Position += Direction = Vector3.Multiply(ViewMatrix.PositiveZ(), amount);*/

		// public void LookAt(Vector3 target) {
		// 	Direction = target - Position;
		// 	Front = Vector3.Normalize(Direction);
		// }
		
		protected override void RecalculateViewMatrix() {
			ViewMatrix = Matrix4x4.CreateLookAt(
				Position,
				Target,
				Up
			);
			
			Matrix4x4.Invert(ViewMatrix, out var ivm);
			InverseViewMatrix = ivm;
		}

		public override void Update() {
			RecalculateViewMatrix();
			
			// Data = new CameraData {
			// 	Projection = ProjectionMatrix,
			// 	View = ViewMatrix,
			// 	Position = Position
			// };
		}
	}
}
