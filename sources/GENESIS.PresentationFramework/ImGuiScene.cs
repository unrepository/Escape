using GENESIS.GPU;
using GENESIS.PresentationFramework.Drawing;
using GENESIS.PresentationFramework.Extensions;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;

namespace GENESIS.PresentationFramework {
	
	public abstract class ImGuiScene : Scene {
		
		protected ImGuiContextPtr ImContext { get; private set; }
		protected ImGuiIOPtr ImIO { get; private set; }

		protected ImGuiScene(string id) : base(default, id) {}

		public override void Initialize(Window window) {
			base.Initialize(window);
			
			ImContext = window.CreateImGui();
			ImGui.SetCurrentContext(ImContext);
			ImIO = ImGui.GetIO();
		}

		public override void Render(double delta) {
			ImGui.SetCurrentContext(ImContext);
			Paint(delta);
		}
	}
}
