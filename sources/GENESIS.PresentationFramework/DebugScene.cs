using System.Numerics;
using GENESIS.PresentationFramework.Drawing;
using Hexa.NET.ImGui;

namespace GENESIS.PresentationFramework {
	
	public class DebugScene : ImGuiScene {

		public static Dictionary<string, Action<double>> DebugInfoSlots = [];
		
		public DebugScene() : base("PresentationFramework/debug") { }

		public override void Render(double delta) {
		#if DEBUG
			base.Render(delta);
		#endif
		}

		protected override void Paint(double delta) {
			ImGui.SetNextWindowSize(new Vector2(300, 400), ImGuiCond.FirstUseEver);
			
			if(ImGui.Begin(Id, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar)) {
				ImGui.BeginTabBar("debug");
			
				foreach(var entry in DebugInfoSlots) {
					var name = entry.Key;
					var action = entry.Value;

					if(ImGui.BeginTabItem(name)) {
						action(delta);
						
						ImGui.EndTabItem();
					}
				}
				
				ImGui.EndTabBar();
			}
			
			ImGui.End();
		}
	}
}
