using Cinenic.Extensions.Debugging.Providers;
using Cinenic.Extensions.ImGui;
using Cinenic.Renderer;
using Hexa.NET.ImGui;
using ImGui_ = Hexa.NET.ImGui.ImGui;

namespace Cinenic.Extensions.Debugging {
	
	public class DebugInterface : IRenderer {

		public const string ID = "debug_std";
		
		public string Id { get; } = ID;
		public int Priority { get; init; }

		public List<DebugInfoProvider> Providers = [];

		private DebugInterface() {
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

				if(ImGui_.Begin(provider.Title, ref provider.IsOpen, ImGuiWindowFlags.HorizontalScrollbar)) {
					provider.Render();
				}
				
				ImGui_.End();
			}
		}

		public static DebugInterface Setup(IPlatform platform) {
			var shaderPipeline = new EmptyShaderPipeline(platform);
			var renderQueue = RenderQueueManager.Create(platform, ID);
			var renderPipeline = RenderPipelineManager.Create(platform, ID, renderQueue, shaderPipeline);

			var window = Window.Create(platform);
			window.Title = "Standard Debugging Interface";
			window.Initialize(renderQueue);
			renderQueue.RenderTarget = window.Framebuffer;
			
			UpdateManager.Add((WindowUpdater) window);

			return Setup(platform, renderQueue, window);
		}
		
		public static DebugInterface Setup(IPlatform platform, RenderQueue queue, Window window) {
			ImGuiController.Create(platform, ID, queue, window);
			var di = new DebugInterface();
			RenderManager.Add(queue, di);
			return di;
		}
	}
}
