using System.Numerics;
using Cinenic.Renderer;
using Cinenic.Renderer.Shader;

namespace Cinenic.Presentation.Camera {
	
	public class OrthographicCamera3D : Camera3D {

		public OrthographicCamera3D(Window window, Shader shader) : base(window, shader) { }
		
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
