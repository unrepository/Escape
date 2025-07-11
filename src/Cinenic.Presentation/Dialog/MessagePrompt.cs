using System.Numerics;
using Hexa.NET.ImGui;

namespace Cinenic.Presentation.Dialog {
	
	public class MessagePrompt : IPromptDialog<MessagePromptResult> {
		
		public bool IsOpen { get; private set; }
		public MessagePromptResult Result { get; private set; }

		private readonly string _title;
		private readonly string _message;
		private readonly Buttons _buttons;

		public MessagePrompt(string title, string message, Buttons buttons) {
			_title = title;
			_message = message;
			_buttons = buttons;

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

				switch(_buttons) {
					case Buttons.YesNo:
						if(ImGui.Button("Yes")) {
							Result = MessagePromptResult.Yes;
							IsOpen = false;
						}

						ImGui.SameLine();
						
						if(ImGui.Button("No")) {
							Result = MessagePromptResult.No;
							IsOpen = false;
						}
						break;
					default:
						throw new NotImplementedException();
				}
				
				if(popup) ImGui.EndPopup();
			}

			if(!popup) ImGui.End();
			return !IsOpen;
		}

		public enum Buttons {
			
			YesNo
		}
	}
	
	public enum MessagePromptResult {
			
		Yes,
		No
	}
}
