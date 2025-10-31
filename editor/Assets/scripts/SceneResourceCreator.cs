using System;
using System.Text;
using Escape.Core;
using Escape.Core.Resources;
using Escape.Extensions.UI.Dialog;
using Escape.Renderer;
using Escape.Resources;
using Hexa.NET.ImGui;

public class SceneResourceCreator : ResourceCreator<SceneResource, Scene, SceneResource.Import> {

	public RenderQueue RenderQueue { get; }
	
	private const string _title = "New scene";
	private string _sceneId = "";
	private bool _renderable = true;

	public SceneResourceCreator(IPlatform platform, string filePath, string resourcePath, RenderQueue renderQueue)
		: base(platform, filePath, resourcePath)
	{
		RenderQueue = renderQueue;
	}

	public override bool Prompt(bool popup = true) {
		if(popup) ImGui.OpenPopup(_title);

		var begin = popup
			? ImGui.BeginPopup(_title, ImGuiWindowFlags.AlwaysAutoResize)
			: ImGui.Begin(_title, ImGuiWindowFlags.AlwaysAutoResize);
		
		if(begin) {
			ImGui.InputText("ID", ref _sceneId, 128);
			ImGui.Checkbox("Renderable", ref _renderable);

			if(ImGui.Button("Create") && !string.IsNullOrWhiteSpace(_sceneId)) {
				var scene = new Scene(Platform, _sceneId, null, _renderable ? RenderQueue : null);
				Result = new SceneResource(Platform, FilePath + ".scene", scene);
				IsOpen = false;
			}
			
			if(popup) ImGui.EndPopup();
		}

		if(!popup) ImGui.End();
		return !IsOpen;
	}
}
