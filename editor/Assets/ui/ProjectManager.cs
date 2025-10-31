using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.Loader;
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

	private FilePrompt _openProjectPrompt = new("Open project...", filters: [ "d" ]);
	
	public override void OnRender(RenderQueue queue, TimeSpan delta) {
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
				_openProjectPrompt.IsOpen = true;
			}
			
			ImGui.End();
		}

		if(_openProjectPrompt?.Prompt() == true) {
			if(!string.IsNullOrWhiteSpace(_openProjectPrompt.Result)) {

				// load project
				ProjectGlobals.ProjectDirectory = new DirectoryInfo(_openProjectPrompt.Result);
				ProjectGlobals.ProjectInfo = ProjectInfo.Load(Path.Combine(ProjectGlobals.ProjectDirectory.FullName, ProjectInfo.FILE_NAME));

				Debug.Assert(ProjectGlobals.ProjectInfo is not null);

			#region Load assemblies
				var loadContext = new AssemblyLoadContext("ProjectLoadContext", true);
				loadContext.Resolving += (context, assemblyName) => {
					string assemblyPath = Path.Combine(ProjectGlobals.OutputDirectory!.FullName, assemblyName + ".dll");

					if(!File.Exists(assemblyPath)) {
						Logger.Warn(
							"{ProjectName}: Could not resolve assembly for {AssemblyName}",
							ProjectGlobals.ProjectInfo.Name, assemblyName
						);
					} else {
						Logger.Info(
							"{ProjectName}: Resolved assembly {AssemblyName} to {AssemblyPath}",
							ProjectGlobals.ProjectInfo.Name, assemblyName, assemblyPath
						);
					}

					using var stream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read);
					return context.LoadFromStream(stream);
				};

				// TODO !!! extensions and dependencies are not being loaded
				ProjectGlobals.ProjectAssembly =
					loadContext.LoadFromAssemblyPath(Path.Combine(ProjectGlobals.OutputDirectory!.FullName,
						ProjectGlobals.ProjectInfo.MainAssemblyName + ".dll"));
			#endregion

				ProjectResources.Load(queue.Platform, ProjectGlobals.ResourcesDirectory!);

				ESCAPE.RenderThread.ScheduleAction(() => {
					SceneEngine.SetScene(queue, new Escape.Editor.Scenes.ProjectEditor(queue.Platform, queue));
				});
			}

			_openProjectPrompt.IsOpen = false;
		}
	}

	public override void OnUpdate(TimeSpan delta) {
		base.OnUpdate(delta);
	}
}
