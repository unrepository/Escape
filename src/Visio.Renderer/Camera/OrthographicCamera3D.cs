using System.Numerics;

namespace Visio.Renderer.Camera {
	
	public class OrthographicCamera3D : Camera3D {

		public OrthographicCamera3D(Framebuffer framebuffer) : base(framebuffer) { }
		public OrthographicCamera3D(Window window) : base(window) { }
		public OrthographicCamera3D(int width, int height) : base(width, height) { }

		protected override void RecalculateProjectionMatrix() {
			ProjectionMatrix = Matrix4x4.CreateOrthographic(
				Width * FieldOfView / 1000,
				Height * FieldOfView / 1000,
				ZNear,
				ZFar
			);
			
			Matrix4x4.Invert(ProjectionMatrix, out var ipm);
			InverseProjectionMatrix = ipm;
		}
	}
}
