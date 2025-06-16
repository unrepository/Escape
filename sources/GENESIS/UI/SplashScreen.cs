using System.Numerics;
using Eclair.Renderer;
using Eclair.Presentation;
using Eclair.Presentation.Dialog;
using Eclair.Presentation.Extensions;
using GENESIS.Project;
using GENESIS.UI.Project;
using Hexa.NET.ImGui;

namespace GENESIS.UI {
	
	public class SplashScreen : ImGuiScene {

		private FilePrompt? _newProjectDialog;

		public SplashScreen(IPlatform platform) : base(platform, "splash_screen") { }
		
		protected override void Paint(double delta) {
		#if DEBUG
			ImGui.ShowDemoWindow();
		#endif
			
			// center splash window
			ImGui.SetNextWindowPos(
				ImGui.GetCenter(ImGui.GetMainViewport()),
				ImGuiCond.Always,
				new Vector2(0.5f, 0.5f)
			);
			
			if(ImGui.Begin("GENESIS",
				   ImGuiWindowFlags.AlwaysAutoResize
				   | ImGuiWindowFlags.NoMove
				   | ImGuiWindowFlags.NoTitleBar))
			{
				if(ImGui.Button("Open Last Project")) {
					throw new NotImplementedException();
				}
				
				if(ImGui.Button("New Project...")) {
					throw new NotImplementedException();
				}

				if(ImGui.Button("Open Project...")) {
					_newProjectDialog = new FilePrompt("Choose a project directory...", filters: [ "d" ]);
				}
			}
			
			ImGui.End();

			if(_newProjectDialog?.Prompt() == true) {
				if(_newProjectDialog.Result is null) return;
				ProjectManager.Load(_newProjectDialog.Result);
				
				Window!.ScheduleLater(() => {
					Window.PushScene(new ProjectScreen(Platform));
					Window.PopScene(this);
				});
			}
		}
	}
}
