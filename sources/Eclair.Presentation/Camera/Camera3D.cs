using System.Numerics;
using Eclair.Renderer;
using Eclair.Renderer.Shader;
using GENESIS.LanguageExtensions;

namespace Eclair.Presentation.Camera {
	
	public abstract class Camera3D : CameraBase {
		
		public static float ZNear { get; set; } = 0.01f;
		public static float ZFar { get; set; } = 1_000_000f;

		public Vector3 Position = Vector3.Zero;
		public Vector3 Rotation = Vector3.Zero;

		public float Yaw {
			get => Rotation.Y;
			set {
				Rotation.Y = value;
				Direction.X = MathF.Cos(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
				Direction.Z = MathF.Sin(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
				Front = Vector3.Normalize(Direction);
			}
		}
		
		public float Pitch {
			get => Rotation.X;
			set {
				Rotation.X = value;
				Direction.X = MathF.Cos(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
				Direction.Y = MathF.Sin(Rotation.X.ToRadians());
				Direction.Z = MathF.Sin(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
				Front = Vector3.Normalize(Direction);
			}
		}
		
		public Vector3 Direction = Vector3.Zero;
		public Vector3 Front = Vector3.UnitZ;
		public Vector3 Up = Vector3.UnitY;

		protected Camera3D(Window window, Shader shader) : base(window, shader) { }
		protected Camera3D(int width, int height, Shader shader) : base(width, height, shader) { }

		/*public void MoveUp(float amount) => Position += Up = Vector3.Multiply(ViewMatrix.PositiveY(), amount);
		public void MoveDown(float amount) => Position -= Up = Vector3.Multiply(ViewMatrix.PositiveY(), amount);
		public void MoveLeft(float amount) => Position -= Vector3.Multiply(ViewMatrix.PositiveX(), amount);
		public void MoveRight(float amount) => Position += Vector3.Multiply(ViewMatrix.PositiveX(), amount);
		public void MoveForward(float amount) => Position -= Direction = Vector3.Multiply(ViewMatrix.PositiveZ(), amount);
		public void MoveBackward(float amount) => Position += Direction = Vector3.Multiply(ViewMatrix.PositiveZ(), amount);*/

		public void LookAt(Vector3 target) {
			Direction = target - Position;
			Front = Vector3.Normalize(Direction);
		}
		
		protected override void RecalculateViewMatrix() {
			ViewMatrix = Matrix4x4.CreateLookAt(
				Position,
				Position + Front,
				Up
			);
			
			Matrix4x4.Invert(ViewMatrix, out var ivm);
			InverseViewMatrix = ivm;
		}

		public override void Update() {
			RecalculateViewMatrix();
			
			ShaderData.Data = new CameraData {
				Projection = ProjectionMatrix,
				View = ViewMatrix,
				Position = Position
			};
			
			ShaderData.Push();
		}
	}
}
