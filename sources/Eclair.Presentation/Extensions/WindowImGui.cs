using Eclair.Renderer;
using Eclair.Renderer.OpenGL;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Silk.NET.Core;
using Silk.NET.Windowing.Glfw;

namespace Eclair.Presentation.Extensions {
	
	public static class WindowImGui {

		private static readonly Dictionary<Window, ImGuiContextPtr> _windowContexts = [];

		public static bool HasImGuiContext(this Window window) 
			=> _windowContexts.ContainsKey(window);

		public static ImGuiContextPtr CreateImGui(this Window window) {
			if(_windowContexts.TryGetValue(window, out var ctx)) {
				return ctx;
			}

			ctx = ImGui.CreateContext();
			ImGui.SetCurrentContext(ctx);

			var io = ImGui.GetIO();
			io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard; // Enable Keyboard Controls
			io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;  // Enable Gamepad Controls
			io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;     // Enable Docking
			io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;   // Enable Multi-Viewport / Platform Windows
			io.ConfigViewportsNoAutoMerge = false;
			io.ConfigViewportsNoTaskBarIcon = false;

			if(window is not GLWindow) {
				throw new NotSupportedException("Non-OpenGL windows are not supported");
			}
			
			ImGuiImplGLFW.SetCurrentContext(ctx);

			unsafe {
				if(!ImGuiImplGLFW.InitForOpenGL(new GLFWwindowPtr((GLFWwindow*) GlfwWindowing.GetHandle(window.Base)), true)) {
					throw new PlatformException("Failed to initialize ImGui for GLFW");
				}
			}
			
			ImGuiImplOpenGL3.SetCurrentContext(ctx);

			if(!ImGuiImplOpenGL3.Init("#version 330")) {
				throw new PlatformException("Failed to initialize ImGui for OpenGL");
			}

			var begin = (double _) => {
				ImGui.SetCurrentContext(window.CreateImGui());
				
				ImGuiImplOpenGL3.NewFrame();
				ImGuiImplGLFW.NewFrame();
				ImGui.NewFrame();
			};
			
			var end = (double _) => {
				ImGui.SetCurrentContext(window.CreateImGui());
				
				ImGui.Render();
				ImGui.EndFrame();
		
				ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());
		
				if((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
					ImGui.UpdatePlatformWindows();
					ImGui.RenderPlatformWindowsDefault();
				}
			};
			
			window.AddRenderQueue(-9999, begin);
			window.AddRenderQueue(9999, end);
			
			_windowContexts[window] = ctx;
			return ctx;
		}
	}
}
