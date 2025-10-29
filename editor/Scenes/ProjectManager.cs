using System.Diagnostics;
using Arch.Core;
using Escape.Core;
using Escape.Core.Scripting.Components;
using Escape.Core.Scripting.Resources;
using Escape.Extensions.UI;
using Escape.Renderer;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Vulkan;
using Escape.Resources;

namespace Escape.Editor.Scenes {
	
	public class ProjectManager : Scene {

		public ProjectManager(IPlatform platform, RenderQueue? renderQueue) : base(platform, "project_manager", null, renderQueue) {
			Debug.Assert(renderQueue is not null);

			var uiScript = ResourceManager.Load<ScriptResource>(platform, "ui/ProjectManager.cs")!;
			World.Create(new Scripted(uiScript, this));
		}
	}
}
