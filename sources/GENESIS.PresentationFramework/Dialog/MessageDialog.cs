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
		
		public bool Prompt(bool popup = true) {
			if(!IsOpen) return false;
			
			ImGui.SetNextWindowPos(
				ImGui.GetCenter(ImGui.GetMainViewport()),
				ImGuiCond.Appearing,
				new Vector2(0.5f, 0.5f)
			);
			
			if(popup) ImGui.OpenPopup(_title);

			var begin = popup
				? ImGui.BeginPopup(_title, ImGuiWindowFlags.AlwaysAutoResize)
				: ImGui.Begin(_title, ImGuiWindowFlags.AlwaysAutoResize);
			
			if(begin) {
				foreach(var messageLine in _message.Split("\n")) {
					ImGui.Text(messageLine);
				}
				
				if(ImGui.Button("OK")) {
					Result = true;
					IsOpen = false;
				}

				if(popup) ImGui.EndPopup();
			}

			if(!popup) ImGui.End();
			return !IsOpen;
		}
	}
}
