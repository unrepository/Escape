using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;

namespace GENESIS.PresentationFramework.Camera {
	
	public class PerspectiveCamera3D : Camera3D {

		public PerspectiveCamera3D(Window window, IShader shader) : base(window, shader) { }
		
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
