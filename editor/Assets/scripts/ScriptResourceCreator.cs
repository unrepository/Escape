using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using Escape.Core.Resources;
using Escape.Core.Scripting;
using Escape.Editor;
using Escape.Extensions.UI.Dialog;
using Escape.Renderer;
using Escape.Resources;
using Hexa.NET.ImGui;

public class ScriptResourceCreator : ResourceCreator<ScriptResource, IScript, ScriptResource.Import> {
	
	private const string _title = "New script";
	
	private static readonly string[] _scriptTypes = [ "C#" ];
	private int _selectedType = 0;

	private string _csharpClassName = "";
	private bool _csharpIsScript = true;
	
	public ScriptResourceCreator(IPlatform platform, string filePath, string resourcePath) : base(platform, filePath, resourcePath) { }

	public override bool Prompt(bool popup = true) {
		if(popup) ImGui.OpenPopup(_title);

		var begin = popup
			? ImGui.BeginPopup(_title, ImGuiWindowFlags.AlwaysAutoResize)
			: ImGui.Begin(_title, ImGuiWindowFlags.AlwaysAutoResize);
		
		if(begin) {
			ImGui.Combo("Type", ref _selectedType, _scriptTypes, _scriptTypes.Length);

			switch(_scriptTypes[_selectedType]) {
				case "C#":
					ImGui.InputText("Class name", ref _csharpClassName, 64);
					ImGui.Checkbox("Script?", ref _csharpIsScript);

					if(ImGui.Button("Create")) {
						var sb = new StringBuilder();
						sb.Append("using System;\n");
						sb.Append("using Arch.Core;\n");
						sb.Append("using Arch.Core.Extensions;\n");
						sb.Append("using Escape.Core.Components;\n");
						sb.Append("using Escape.Core.Scripting;\n");
						sb.Append("\n");

						if(_csharpIsScript) {
							sb.Append($"[CSharpScript(\"{ResourcePath + ".cs"}\")]\n");
							sb.Append($"public class {_csharpClassName} : CSharpScript {{ }}\n");
						} else {
							sb.Append($"public class {_csharpClassName} {{ }}\n");
						}

						File.WriteAllText(FilePath + ".cs", sb.ToString());
						
						var res = ResourceManager.LoadByPath<ScriptResource>(
							Platform,
							FilePath + ".cs",
							assembly: ProjectGlobals.ProjectAssembly,
							explicitPath: true
						);
						
						Debug.Assert(res is not null);

						Result = res.Get();
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
}
