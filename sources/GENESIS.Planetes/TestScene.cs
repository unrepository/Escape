using GENESIS.PresentationFramework;
using GENESIS.PresentationFramework.Drawing;
using Hexa.NET.ImGui;

namespace GENESIS.Planetes {
	
	public class TestScene : ImGuiScene {

		public TestScene(Painter painter) : base(painter, "test") { }
		
		protected override void Paint(double delta) {
			ImGui.Text("meow meow");
		}
	}
}
