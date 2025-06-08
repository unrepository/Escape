using System.Numerics;
using Hexa.NET.ImGui;

namespace GENESIS.PresentationFramework.Dialog {
	
	public class MessageDialog : IPromptDialog<bool> {
		
		public bool IsOpen { get; private set; }
		public bool Result { get; private set; }

		private readonly string _title;
		private readonly string _message;

		public MessageDialog(string title, string message) {
			_title = title;
			_message = message;

			IsOpen = true;
		}
		
		public bool Prompt() {
			if(!IsOpen) return false;
			
			ImGui.SetNextWindowPos(
				ImGui.GetCenter(ImGui.GetMainViewport()),
				ImGuiCond.Appearing,
				new Vector2(0.5f, 0.5f)
			);
			
			if(ImGui.Begin(_title, ImGuiWindowFlags.AlwaysAutoResize)) {
				foreach(var messageLine in _message.Split("\n")) {
					ImGui.Text(messageLine);
				}
				
				if(ImGui.Button("OK")) {
					Result = true;
					IsOpen = false;
				}
			}
			ImGui.End();

			return !IsOpen;
		}
	}
}
