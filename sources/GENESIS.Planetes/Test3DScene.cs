using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.PresentationFramework;
using GENESIS.PresentationFramework.Drawing;

namespace GENESIS.Planetes {
	
	public class Test3DScene : EnvironmentScene {

		public Test3DScene(GLPlatform platform, Painter painter) : base(platform, painter, Type.ThreeDimensional, "test3d") { }

		public override void Initialize(Window window) {
			base.Initialize(window);
			
			
		}

		protected override void Paint(double delta) {
			
		}
	}
}
