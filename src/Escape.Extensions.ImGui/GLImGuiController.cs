using System.Diagnostics;
using Escape.Renderer;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Vulkan;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.ImGui.Backends.Vulkan;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Framebuffer = Escape.Renderer.Framebuffer;
using HImGui = Hexa.NET.ImGui.ImGui;
using VkDevice = Hexa.NET.ImGui.Backends.Vulkan.VkDevice;

namespace Escape.Extensions.ImGui {
	
	public class GLImGuiController : ImGuiController {
		
		public unsafe GLImGuiController(string id, GLPlatform platform, RenderQueue queue, Window window) : base(id, platform, queue) {
			//Debug.Assert(queue.RenderTarget is not null, "Render queue must have a target set!");
			Debug.Assert(queue.RenderTarget == window.Framebuffer, "Window must belong to the render queue!");
			
			ImGuiImplGLFW.SetCurrentContext(Context);

			if(!ImGuiImplGLFW.InitForOpenGL(new GLFWwindowPtr((GLFWwindow*) window.Base.Handle), true)) {
				throw new PlatformException("Could not initialize the GLFW backend for ImGui");
			}
			
			ImGuiImplOpenGL3.SetCurrentContext(Context);

			if(!ImGuiImplOpenGL3.Init("#version 330")) {
				throw new PlatformException("Could not initialize the OpenGL 3 backend for ImGui");
			}
		}

		public override void Begin() {
			ImGuiImplGLFW.SetCurrentContext(Context);
			ImGuiImplOpenGL3.SetCurrentContext(Context);
			HImGui.SetCurrentContext(Context);
			
			ImGuiImplGLFW.NewFrame();
			ImGuiImplOpenGL3.NewFrame();
			HImGui.NewFrame();
		}
		
		public override void End() {
			ImGuiImplGLFW.SetCurrentContext(Context);
			ImGuiImplOpenGL3.SetCurrentContext(Context);
			HImGui.SetCurrentContext(Context);
			
			HImGui.Render();
			
			ImGuiImplOpenGL3.RenderDrawData(HImGui.GetDrawData());
			
			if((IO.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
				HImGui.UpdatePlatformWindows();
				HImGui.RenderPlatformWindowsDefault();
			}
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
