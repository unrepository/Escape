using System;
using System.IO;
using Escape.Core;
using Escape.Core.Scripting;
using Escape.Editors.ResourceEditor;
using Escape.Extensions.UI;
using Escape.Extensions.UI.Dialog;
using Escape.Renderer;
using Hexa.NET.ImGui;

[CSharpScript("scripts/EditorUI.cs")]
public class EditorUI : CSharpScript {

	private FilePrompt? _assetsOpenPrompt;
	private FileInfo? _selectedFile;
	
	public EditorUI() { }
	
	public override void OnRender(TimeSpan delta, ObjectRenderer objectRenderer) {
		base.OnRender(delta, objectRenderer);
		
		ImGui.ShowDemoWindow();
		
		ImGui.Begin("Resource Editor", ImGuiWindowFlags.MenuBar);

		if(ImGui.BeginMenuBar()) {
			if(ImGui.BeginMenu("Assets")) {
				if(ImGui.Button("Open directory...")) {
					_assetsOpenPrompt = new FilePrompt("Open assets directory", filters: [ "d" ]);
				}
				
				ImGui.EndMenu();
			}
			
			ImGui.EndMenuBar();
		}

		if(EditorGlobals.AssetsDirectory is not null) {
			ImGui.Columns(2, true);

			void DrawDirectoryTree(DirectoryInfo directory) {
				ImGui.Text(directory.Name);
				ImGui.TreePush(directory.Name);

				foreach(var subDirectory in directory.EnumerateDirectories()) {
					DrawDirectoryTree(subDirectory);
				}
				
				foreach(var file in directory.EnumerateFiles()) {
					if(ImGui.Button(file.Name)) {
						_selectedFile = file;
						//RenderManager.Add(objectRenderer);
					}
				}
				
				ImGui.TreePop();
			}
			
			DrawDirectoryTree(EditorGlobals.AssetsDirectory);
		}
		
		ImGui.End();

		if(_assetsOpenPrompt?.Prompt() == true) {
			if(_assetsOpenPrompt.Result is null) return;
			EditorGlobals.AssetsDirectory = new DirectoryInfo(_assetsOpenPrompt.Result);
		}
	}

	public override void OnUpdate(TimeSpan delta) {
		base.OnUpdate(delta);
	}
}
