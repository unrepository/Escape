using System.Diagnostics;
using Arch.Core;
using Escape.Core;
using Escape.Core.Scripting.Components;
using Escape.Core.Scripting.Resources;
using Escape.Editor.Windows;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Editor.Scenes {
	
	public class ProjectEditor : Scene {

		public static List<IEditorWindow> OpenWindows { get; } = [];

		public ProjectEditor(IPlatform platform, RenderQueue? renderQueue) : base(platform, "project_editor", null, renderQueue) {
			Debug.Assert(renderQueue is not null);

			var uiScript = ResourceManager.Load<ScriptResource>(platform, "ui/ProjectEditor.cs")!;
			World.Create(new Scripted(uiScript));
		}
	}
}
