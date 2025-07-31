using System.Numerics;
using Visio.Extensions.CSharp;

namespace Visio.Renderer.Camera {
	
	public abstract class Camera3D : CameraBase {
		
		public static float ZNear { get; set; } = 0.01f;
		public static float ZFar { get; set; } = 1_000f;

		public Vector3 Position = Vector3.Zero;
		public Vector3 Target = Vector3.Zero;
		public Vector3 Up = Vector3.UnitY;

		protected Camera3D(Framebuffer framebuffer) : base(framebuffer) { }
		protected Camera3D(Window window) : base(window) { }
		protected Camera3D(int width, int height) : base(width, height) { }
		
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
		}
	}
}
