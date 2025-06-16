using System.Numerics;
using Eclair.Renderer;
using Eclair.Renderer.Shader;

namespace Eclair.Presentation.Camera {
	
	public class Camera2D : CameraBase {

		public Vector2 Position = Vector2.Zero;
		
		public Vector3 Direction = Vector3.Zero;
		public Vector3 Front = Vector3.UnitZ;
		public Vector3 Up = Vector3.UnitY;

		public Camera2D(Window window, Shader shader) : base(window, shader) { }
		public Camera2D(int width, int height, Shader shader) : base(width, height, shader) { }
		
		protected override void RecalculateProjectionMatrix() {
			ProjectionMatrix = Matrix4x4.CreateOrthographic(
				-Width,
				-Height,
				0.0f,
				1.0f
			);
			
			Matrix4x4.Invert(ProjectionMatrix, out var ipm);
			InverseProjectionMatrix = ipm;
		}
		
		protected override void RecalculateViewMatrix() {
			ViewMatrix = Matrix4x4.CreateLookAt(
				new Vector3(Position.X, Position.Y, 0.0f),
				new Vector3(Position.X, Position.Y, 0) + Front,
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
				Position = new Vector3(Position.X, Position.Y, 0.0f)
			};
			
			ShaderData.Push();
		}
	}
}
