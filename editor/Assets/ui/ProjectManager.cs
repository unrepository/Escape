using System;
using System.IO;
using System.Numerics;
using Escape.Core;
using Escape.Core.Scripting;
using Escape.Editor;
using Escape.Extensions.CSharp;
using Escape.Extensions.UI;
using Escape.Extensions.UI.Dialog;
using Escape.Renderer;
using Hexa.NET.ImGui;

[CSharpScript("ui/ProjectManager.cs")]
public class ProjectManager : CSharpScript {

	private readonly Scene _parent;
	private FilePrompt? _openProjectPrompt;

	public ProjectManager(Scene parent) {
		_parent = parent;
	}
	
	public override void OnRender(TimeSpan delta, ObjectRenderer objectRenderer) {
		base.OnRender(delta, objectRenderer);
		
	#if DEBUG
		ImGui.ShowDemoWindow();
	#endif
		
		// center window
		ImGui.SetNextWindowPos(
			ImGui.GetCenter(ImGui.GetMainViewport()),
			ImGuiCond.Always,
			new Vector2(0.5f, 0.5f)
		);

		if(
			ImGui.Begin(
			   "Project Manager",
			   ImGuiWindowFlags.AlwaysAutoResize
			   | ImGuiWindowFlags.NoMove
			   | ImGuiWindowFlags.NoTitleBar
		   )
		) {
			if(ImGui.Button("Open project...")) {
				_openProjectPrompt = new FilePrompt("Open project...", filters: [ "d" ]);
			}
			
			ImGui.End();
		}

		if(_openProjectPrompt?.Prompt() == true) {
			if(_openProjectPrompt.Result is null) return;
			EditorGlobals.ProjectDirectory = new DirectoryInfo(_openProjectPrompt.Result);
			
			ESCAPE.RenderThread.ScheduleAction(() => {
				SceneEngine.SetScene(_parent.RenderQueue!, new Escape.Editor.Scenes.ProjectEditor(_parent.Platform, _parent.RenderQueue));
			});
		}
	}

	public override void OnUpdate(TimeSpan delta) {
		base.OnUpdate(delta);
	}
}
