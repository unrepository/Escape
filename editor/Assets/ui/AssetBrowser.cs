using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Escape.Core;
using Escape.Core.Resources;
using Escape.Core.Scripting;
using Escape.Core.Scripting.Components;
using Escape.Editor;
using Escape.Extensions.UI;
using Escape.Extensions.UI.Dialog;
using Escape.Renderer;
using Escape.Renderer.Resources;
using Escape.Renderer.Shader;
using Escape.Resources;
using Hexa.NET.ImGui;

[CSharpScript("ui/AssetBrowser.cs")]
public class AssetBrowser : CSharpScript {

	private FileInfo? _selectedFile;
	private bool _viewExternal = false;

	private TextPrompt? _newResourcePrompt;
	private string? _newResourcePath;
	private ResourceRegistry.Format _newResourceFormat;

	private dynamic? _resourceCreator;
	
	public override void OnRender(RenderQueue queue, TimeSpan delta) {
		if(ImGui.Begin("Resource Editor", ImGuiWindowFlags.MenuBar)) {
			if(ImGui.BeginMenuBar()) {
				if(ImGui.BeginMenu("Resources")) {
					if(ImGui.MenuItem("Rebuild database")) {
						//_assetsOpenPrompt = new FilePrompt("Open assets directory", filters: [ "d" ]);
					}

					ImGui.EndMenu();
				}

				if(ImGui.BeginMenu("Type")) {
					if(ImGui.MenuItem("External", "", _viewExternal)) _viewExternal = true;
					if(ImGui.MenuItem("Internal", "", !_viewExternal)) _viewExternal = false;
					
					ImGui.EndMenu();
				}

				ImGui.EndMenuBar();
			}
			
			Debug.Assert(ProjectGlobals.ResourcesDirectory is not null);

			ImGui.Columns(2, true);
			
			if(!_viewExternal) {
				void DrawDirectoryTree(DirectoryInfo directory) {
					ImGui.Selectable(directory.Name);

					if(ImGui.BeginPopupContextItem()) {
						if(ImGui.BeginMenu("New...")) {
							foreach(var (formatId, format) in ResourceRegistry.Formats) {
								if(format.NewConstructor is null) continue;
								//if(format.ValueConstructor is null) continue;
								
								if(ImGui.MenuItem(formatId)) {
									_newResourcePrompt = new TextPrompt("New resource...", "Name") {
										IsOpen = true
									};
									_newResourcePath = directory.FullName;
									_newResourceFormat = format;
								}
							}
							
							ImGui.EndMenu();
						}
						
						ImGui.EndPopup();
					}
					
					ImGui.TreePush(directory.Name);

					foreach(var subDirectory in directory.EnumerateDirectories()) {
						DrawDirectoryTree(subDirectory);
					}

					foreach(var file in directory.EnumerateFiles()) {
						if(file.Name.EndsWith(ImportMetadata.FILE_EXTENSION)) continue;
						if(!ProjectResources.AllResources.ContainsKey(file.FullName)) continue;

						var resource = ProjectResources.AllResources[file.FullName];

						if(ImGui.Selectable(file.Name)) {
							_selectedFile = file;
						}

						if(ImGui.BeginPopupContextItem()) {
							if(ImGui.Selectable("Open in editor")) {
								switch(resource.Get().Value) {
									case Scene scene:
										World.Create(new Renderable(), new Scripted(EditorResources.SceneEditorScript, Platform, scene));
										break;
									default:
										throw new NotImplementedException();
								}
							}

							ImGui.EndPopup();
						}
					}

					ImGui.TreePop();
				}

				DrawDirectoryTree(ProjectGlobals.ResourcesDirectory);
			} else {
				foreach(var (formatId, format) in ResourceRegistry.Formats) {
					foreach(var type in ProjectGlobals.ProjectAssembly!.GetExportedTypes()) {
						if(type.IsAssignableTo(format.ValueType)) {
							ImGui.Text(type.Name);
						}
					}
				}
			}
			
			ImGui.NextColumn();

			// import metadata viewer/editor
			if(_selectedFile is not null && ProjectResources.AllResources.TryGetValue(_selectedFile.FullName, out var resource)) {
				ImGui.Text(resource.ToString());
			}

			if(_newResourcePrompt?.Prompt() == true) {
				if(!string.IsNullOrWhiteSpace(_newResourcePrompt.Result)) {
					var filePath = Path.Combine(_newResourcePath!, _newResourcePrompt.Result);
					var resourcePath = filePath.Replace(ProjectGlobals.ResourcesDirectory.FullName, "");
					var resourceType = _newResourceFormat.ResourceType;

					_resourceCreator = resourceType switch {
						_ when resourceType == typeof(ScriptResource) =>
							new ScriptResourceCreator(Platform, filePath, resourcePath) {
								IsOpen = true
							},
						_ when resourceType == typeof(SceneResource) =>
							new SceneResourceCreator(Platform, filePath, resourcePath, queue) {
								IsOpen = true
							},
						_ => throw new NotImplementedException()
					};

					_newResourcePath = filePath;
				}
				
				_newResourcePrompt.IsOpen = false;
			}

			if(_resourceCreator?.Prompt() == true) {
				if(_resourceCreator.Result is not null) {
					var res = _resourceCreator.Result;
					res.Save(false);

					ProjectResources.Load(queue.Platform, ProjectGlobals.ResourcesDirectory, clear: false);
				}
				
				_resourceCreator = null;
			}

			ImGui.End();
		}
	}
}
