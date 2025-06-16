using System.Numerics;
using Eclair.Renderer;
using Eclair.Renderer.Shader;
using Eclair.Extensions.CSharp;

namespace Eclair.Presentation.Camera {
	
	public class PerspectiveCamera3D : Camera3D {

		public PerspectiveCamera3D(Window window, Shader shader) : base(window, shader) { }
		
		protected override void RecalculateProjectionMatrix() {
			ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
				FieldOfView.ToRadians(),
				Width / Height,
				ZNear,
				ZFar
			);
			
			Matrix4x4.Invert(ProjectionMatrix, out var ipm);
			InverseProjectionMatrix = ipm;
		}
	}
}
