using GENESIS.GPU;
using GENESIS.PresentationFramework;
using GENESIS.PresentationFramework.Extensions;
using Hexa.NET.ImGui;

namespace GENESIS.UI.Project {
	
	public class ProjectScreen : ImGuiScene {
		
		public ObjectManagerWindow ObjectManager { get; }

		public ProjectScreen(IPlatform platform) : base(platform, "project_screen") {
			ObjectManager = new(platform, this);
		}

		public override void Initialize(Window window) {
			base.Initialize(window);
			
			window.PushScene(ObjectManager);
		}

		public override void Deinitialize(Window window) {
			base.Deinitialize(window);
			
			window.PopScene(ObjectManager);
		}

		protected override void Paint(double delta) {
			ImGui.BeginMainMenuBar();
			{
				if(ImGui.BeginMenu("File")) {
					if(ImGui.MenuItem("Quit")) {
						Window!.ScheduleLater(() => Window.Close());
					}
					
					ImGui.EndMenu();
				}
			}
			ImGui.EndMainMenuBar();
		}
	}
}
