using System.Numerics;
using Hexa.NET.ImGui;

namespace Escape.Extensions.UI.Dialog {
	
	public class TextPrompt : IPromptDialog<string> {

		public bool IsOpen { get; set; }
		public string? Result { get; private set; }

		private readonly string _title;
		private readonly string _hint;

		public TextPrompt(string title, string hint) {
			_title = title;
			_hint = hint;
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
				string result = "";
				
				ImGui.SetKeyboardFocusHere();
				if(ImGui.InputTextWithHint("##input", _hint, ref result, 1024,
					   ImGuiInputTextFlags.EnterReturnsTrue))
				{
					Result = result;
					IsOpen = false;
				}

				if(ImGui.Button("Cancel")) {
					Result = null;
					IsOpen = false;
				}
				
				if(popup) ImGui.EndPopup();
			}

			if(!popup) ImGui.End();
			return !IsOpen;
		}
	}
}
