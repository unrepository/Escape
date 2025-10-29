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

namespace Escape.Editor {
	
	public class EditorScene : Scene {
		
		public EditorScene(IPlatform platform, RenderQueue? renderQueue) : base(platform, "resource_editor", null, renderQueue) {
			Debug.Assert(renderQueue is not null);
			
			Window renderWindow = renderQueue.RenderTarget switch {
				GLWindow.WindowFramebuffer glWindowFramebuffer => glWindowFramebuffer.Window,
				VkWindow.WindowFramebuffer vkWindowFramebuffer => vkWindowFramebuffer.Window,
				_ => throw new ArgumentException()
			};
			
			EditorGlobals.ImGuiController = ImGuiController.Create(platform, "main", renderQueue, renderWindow);

			var editorUi = ResourceManager.Load<ScriptResource>(platform, "scripts/EditorUI.cs")!;
			var uiEntity = World.Create(new Scripted(editorUi));
		}
	}
}
