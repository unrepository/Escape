using System;
using System.IO;
using Escape.Core;
using Escape.Core.Scripting;
using Escape.Editor;
using Escape.Extensions.UI;
using Escape.Extensions.UI.Dialog;
using Escape.Renderer;
using Hexa.NET.ImGui;

[CSharpScript("scripts/EditorUI.cs")]
public class AssetBrowser : CSharpScript {

	private FilePrompt? _assetsOpenPrompt;
	private FileInfo? _selectedFile;
	
	public AssetBrowser() { }
	
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

		if(EditorGlobals.ProjectDirectory is not null) {
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
			
			DrawDirectoryTree(EditorGlobals.ProjectDirectory);
		}
		
		ImGui.End();

		if(_assetsOpenPrompt?.Prompt() == true) {
			if(_assetsOpenPrompt.Result is null) return;
			EditorGlobals.ProjectDirectory = new DirectoryInfo(_assetsOpenPrompt.Result);
		}
	}

	public override void OnUpdate(TimeSpan delta) {
		base.OnUpdate(delta);
	}
}
