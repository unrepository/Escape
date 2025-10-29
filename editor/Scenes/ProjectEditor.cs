using System.Diagnostics;
using Arch.Core;
using Arch.Core.Extensions;
using Escape.Core;
using Escape.Core.Components;
using Escape.Core.Scripting.Components;
using Escape.Core.Scripting.Resources;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Editor.Scenes {
	
	public class ProjectEditor : Scene {

		public ProjectEditor(IPlatform platform, RenderQueue? renderQueue) : base(platform, "project_editor", null, renderQueue) {
			Debug.Assert(renderQueue is not null);

			World.GetRootEntity().Add(new Renderable(), new Scripted(EditorResources.ProjectEditorScript));
			World.Create(new Renderable(), new Scripted(EditorResources.AssetBrowserScript), new State(name: "Asset Browser"));
		}
	}
}
