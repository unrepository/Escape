using Cinenic.Renderer.Vulkan;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.Vulkan;
using HImGui = Hexa.NET.ImGui.ImGui;

namespace Cinenic.Extensions.ImGui {
	
	public class VkImGuiRenderQueue : VkRenderQueue {
		
		public required ImGuiController Controller { get; init; }

		public VkImGuiRenderQueue(VkPlatform platform, Family family, Format format) : base(platform, family, format) { }

		public override void Render(TimeSpan delta) {
			ImGuiImplGLFW.SetCurrentContext(Controller.Context);
			ImGuiImplVulkan.SetCurrentContext(Controller.Context);
			HImGui.SetCurrentContext(Controller.Context);
			
			ImGuiImplGLFW.NewFrame();
			ImGuiImplVulkan.NewFrame();
			HImGui.NewFrame();
			
			base.Render(delta);
			
			ImGuiImplGLFW.SetCurrentContext(Controller.Context);
			ImGuiImplVulkan.SetCurrentContext(Controller.Context);
			HImGui.SetCurrentContext(Controller.Context);
			
			HImGui.Render();

			var pipeline = (VkRenderPipeline) Controller.Pipeline;
			var queue = (VkRenderQueue) Controller.Queue;

			pipeline.Begin();
			
			ImGuiImplVulkan.RenderDrawData(
				HImGui.GetDrawData(),
				new(queue.CommandBuffer.Handle),
				default
			);

			pipeline.End();
			
			if((Controller.IO.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
				HImGui.UpdatePlatformWindows();
				HImGui.RenderPlatformWindowsDefault();
			}
		}
	}
}
