using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;

namespace GENESIS.PresentationFramework.Camera {
	
	public abstract class Camera3D : CameraBase {
		
		public static float ZNear { get; set; } = 0.01f;
		public static float ZFar { get; set; } = 1000f;

		public Vector3 Position = Vector3.Zero;
		public Vector3 Rotation = Vector3.Zero;

		public float Yaw {
			get => Rotation.Y;
			set {
				Rotation.Y = value;
				_direction.X = MathF.Cos(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
				_direction.Z = MathF.Sin(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
				_front = Vector3.Normalize(_direction);
			}
		}
		
		public float Pitch {
			get => Rotation.X;
			set {
				Rotation.X = value;
				_direction.X = MathF.Cos(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
				_direction.Y = MathF.Sin(Rotation.X.ToRadians());
				_direction.Z = MathF.Sin(Rotation.Y.ToRadians()) * MathF.Cos(Rotation.X.ToRadians());
				_front = Vector3.Normalize(_direction);
			}
		}
		
		private Vector3 _direction = Vector3.Zero;
		private Vector3 _front = Vector3.UnitZ;
		private Vector3 _up = Vector3.UnitY;

		protected Camera3D(Window window, IShader shader) : base(window, shader) { }

		public void MoveUp(float amount) => Position += _up = Vector3.Multiply(ViewMatrix.PositiveY(), amount);
		public void MoveDown(float amount) => Position -= _up = Vector3.Multiply(ViewMatrix.PositiveY(), amount);
		public void MoveLeft(float amount) => Position -= Vector3.Multiply(ViewMatrix.PositiveX(), amount);
		public void MoveRight(float amount) => Position += Vector3.Multiply(ViewMatrix.PositiveX(), amount);
		public void MoveForward(float amount) => Position -= _direction = Vector3.Multiply(ViewMatrix.PositiveZ(), amount);
		public void MoveBackward(float amount) => Position += _direction = Vector3.Multiply(ViewMatrix.PositiveZ(), amount);

		protected override void RecalculateViewMatrix() {
			ViewMatrix = Matrix4x4.CreateLookAt(
				Position,
				Position + _front,
				_up
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
		}
	}
}
