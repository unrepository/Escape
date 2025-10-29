using System;
using System.IO;
using Escape.Core;
using Escape.Core.Scripting;
using Escape.Editor;
using Escape.Extensions.UI;
using Escape.Extensions.UI.Dialog;
using Escape.Renderer;
using Escape.Resources;
using Hexa.NET.ImGui;

[CSharpScript("ui/AssetBrowser.cs")]
public class AssetBrowser : CSharpScript {

	private FileInfo? _selectedFile;
	
	public override void OnRender(RenderQueue queue, TimeSpan delta) {
		ImGui.Begin("Resource Editor", ImGuiWindowFlags.MenuBar);

		if(ImGui.BeginMenuBar()) {
			if(ImGui.BeginMenu("Resources...")) {
				if(ImGui.Button("Rebuild database")) {
					//_assetsOpenPrompt = new FilePrompt("Open assets directory", filters: [ "d" ]);
				}
				
				ImGui.EndMenu();
			}
			
			ImGui.EndMenuBar();
		}

		if(ProjectGlobals.ResourcesDirectory is not null) {
			ImGui.Columns(2, true);

			void DrawDirectoryTree(DirectoryInfo directory) {
				ImGui.Text(directory.Name);
				ImGui.TreePush(directory.Name);

				foreach(var subDirectory in directory.EnumerateDirectories()) {
					DrawDirectoryTree(subDirectory);
				}
				
				foreach(var file in directory.EnumerateFiles()) {
					if(file.Name.EndsWith(ImportMetadata.FILE_EXTENSION)) continue;
					
					if(ImGui.Selectable(file.Name)) {
						_selectedFile = file;
					}

					if(ImGui.BeginPopupContextItem()) {
						if(ImGui.Selectable("Open in editor")) {
							// file editor
						}
						
						ImGui.EndPopup();
					}
				}
				
				ImGui.TreePop();
			}
			
			DrawDirectoryTree(ProjectGlobals.ResourcesDirectory);
			
			ImGui.NextColumn();
			
			// import metadata viewer/editor
			if(_selectedFile is not null) {
				var resource = ProjectResources.AllResources[_selectedFile];
				ImGui.Text(resource.ToString());
			}
		}
		
		ImGui.End();
	}
}
