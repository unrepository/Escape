using Cinenic.Extensions.Debugging.Providers;
using Cinenic.Extensions.ImGui;
using Cinenic.Renderer;
using Hexa.NET.ImGui;
using Silk.NET.Windowing;
using ImGui_ = Hexa.NET.ImGui.ImGui;
using Window = Cinenic.Renderer.Window;

namespace Cinenic.Extensions.Debugging {
	
	public class DebugInterface : IRenderer {

		public const string ID = "debug_std";
		
		public string Id { get; } = ID;
		public int Priority { get; init; }

		public List<DebugInfoProvider> Providers = [];

		private ImGuiController _controller;

		private DebugInterface(ImGuiController controller) {
			_controller = controller;
			Providers.Add(new DemoProvider());
		}
		
		public void Render(RenderQueue queue, TimeSpan delta) {
			if(ImGui_.BeginMainMenuBar()) {
				if(ImGui_.BeginMenu("Debug Interface")) {
					ImGui_.Text(":3");
					ImGui_.EndMenu();
				}

				if(ImGui_.BeginMenu("Providers")) {
					foreach(var provider in Providers) {
						if(ImGui_.MenuItem(provider.Title, "", ref provider.IsOpen)) { }
					}
					
					ImGui_.EndMenu();
				}
				
				ImGui_.EndMainMenuBar();
			}

			foreach(var provider in Providers) {
				if(!provider.IsOpen) continue;
				provider.Controller = _controller;

				if(provider is TransformGizmoProvider) {
					var viewport = ImGui_.GetMainViewport();
					ImGui_.SetNextWindowPos(viewport.WorkPos);
					ImGui_.SetNextWindowSize(viewport.WorkSize);
				}

				if(ImGui_.Begin(provider.Title, ref provider.IsOpen, provider.WindowFlags)) {
					provider.Render();
				}
				
				ImGui_.End();
			}
		}

		public static DebugInterface Setup(IPlatform platform) {
			var shaderPipeline = new EmptyShaderPipeline(platform);
			var renderQueue = RenderQueueManager.Create(platform, ID);
			var renderPipeline = RenderPipelineManager.Create(platform, ID, renderQueue, shaderPipeline);

			var window = Window.Create(platform, WindowOptions.Default with {
				TransparentFramebuffer = true
			});
			window.Title = "Standard Debugging Interface";
			window.Initialize(renderQueue);
			renderQueue.RenderTarget = window.Framebuffer;

			return Setup(platform, renderQueue, window);
		}
		
		public static DebugInterface Setup(IPlatform platform, RenderQueue queue, Window window) {
			var controller = ImGuiController.Create(platform, ID, queue, window);
			var di = new DebugInterface(controller);
			
			RenderManager.Add(queue, di);
			return di;
		}
	}
}
