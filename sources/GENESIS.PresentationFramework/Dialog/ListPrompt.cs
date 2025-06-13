using System.Numerics;
using Hexa.NET.ImGui;

namespace GENESIS.PresentationFramework.Dialog {
	
	public class ListPrompt : IPromptDialog<string> {

		public bool IsOpen { get; private set; }
		public string Result { get; private set; }

		private readonly string _title;
		private readonly string _label;
		private readonly string[] _choices;

		private int _selectedItem = 0;

		public ListPrompt(string title, string label, params string[] choices) {
			_title = title;
			_label = label;
			_choices = choices;

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
				ImGui.Combo(_label, ref _selectedItem, string.Join('\0', _choices));

				if(ImGui.Button("OK")) {
					Result = _choices[_selectedItem];
					IsOpen = false;
				}

				if(popup) ImGui.EndPopup();
			}

			if(!popup) ImGui.End();
			return !IsOpen;
		}
	}
}
