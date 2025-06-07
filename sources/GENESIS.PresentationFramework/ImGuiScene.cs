using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.PresentationFramework.Drawing;
using GENESIS.PresentationFramework.Drawing.OpenGL;
using GENESIS.PresentationFramework.Extensions;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;

namespace GENESIS.PresentationFramework {
	
	public abstract class ImGuiScene : Scene {
		
		public IPlatform Platform { get; }
		
		protected ImGuiContextPtr ImContext { get; private set; }
		protected ImGuiIOPtr ImIO { get; private set; }

		protected ImGuiScene(IPlatform platform, string id) : base(default, id) {
			Platform = platform;
		}

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
