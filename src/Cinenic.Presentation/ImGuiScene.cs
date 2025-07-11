using Cinenic.Renderer;
using Cinenic.Renderer.OpenGL;
using Cinenic.Presentation.Drawing;
using Cinenic.Presentation.Drawing.OpenGL;
using Cinenic.Presentation.Extensions;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;

namespace Cinenic.Presentation {
	
	public abstract class ImGuiScene : Scene {
		
		public IPlatform Platform { get; }
		
		protected ImGuiContextPtr? ImContext { get; private set; }
		protected ImGuiIOPtr ImIO { get; private set; }

		protected bool IsOpen = true;

		protected ImGuiScene(IPlatform platform, string id) : base(default, id) {
			Platform = platform;
		}

		public override void Initialize(Window window) {
			base.Initialize(window);
			
			ImContext = window.CreateImGui();
			ImGui.SetCurrentContext(ImContext.Value);
			ImIO = ImGui.GetIO();
		}

		public override void Deinitialize(Window window) {
			base.Deinitialize(window);

			if(!ImContext.HasValue) return;
			
			ImGui.SetCurrentContext(ImContext.Value);
			unsafe { ImGui.SaveIniSettingsToDisk(ImIO.IniFilename); }
		}

		public override void Update(double delta) {
			base.Update(delta);
			
			if(!ImContext.HasValue) return;
			
			ImGui.SetCurrentContext(ImContext.Value);
			ImIO = ImGui.GetIO();
		}

		public override void Render(double delta) {
			if(!ImContext.HasValue) return;
			ImGui.SetCurrentContext(ImContext.Value);

			if(!IsOpen) {
				Window!.ScheduleLater(() => Window.PopScene(this));
				return;
			}
			
			Paint(delta);
		}
	}
}
