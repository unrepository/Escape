using System.Diagnostics;
using Cinenic.Renderer;
using Cinenic.Renderer.Vulkan;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.Vulkan;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Framebuffer = Cinenic.Renderer.Framebuffer;
using HImGui = Hexa.NET.ImGui.ImGui;
using VkDevice = Hexa.NET.ImGui.Backends.Vulkan.VkDevice;

namespace Cinenic.Extensions.ImGui {
	
	public class VkImGuiController : ImGuiController {
		
		private readonly VkPlatform _platform;
		private readonly VkRenderQueue _queue;
		private readonly VkRenderPipeline _pipeline;
		
		public unsafe VkImGuiController(string id, VkPlatform platform, RenderQueue queue, Window window) : base(id, platform, queue) {
			_platform = platform;
			_queue = (VkRenderQueue) queue;
			
			//Debug.Assert(queue.RenderTarget is not null, "Render queue must have a target set!");
			Debug.Assert(queue.RenderTarget == window.Framebuffer, "Window must belong to the render queue!");
			
			ImGuiImplGLFW.SetCurrentContext(Context);

			if(!ImGuiImplGLFW.InitForVulkan(new GLFWwindowPtr((GLFWwindow*) window.Base.Handle), true)) {
				throw new PlatformException("Could not initialize the GLFW backend for ImGui");
			}
			
			var info = new ImGuiImplVulkanInitInfo {
				ApiVersion = Vk.Version10,
				Instance = new(_platform.Vk.Handle),
				PhysicalDevice = new(_platform.PrimaryDevice.Physical.Handle),
				Device = new(_platform.PrimaryDevice.Logical.Handle),
				QueueFamily = (uint) _platform.PrimaryDevice.GraphicsFamily,
				Queue = new(_platform.PrimaryDevice.GraphicsQueue.Handle),
				DescriptorPoolSize = 16,
				RenderPass = new((nint) _queue.Base.Handle),
				MinImageCount = 2,
				ImageCount = (uint) _queue.CommandBuffers.Length,
				MSAASamples = 1,
				Subpass = 0,
			};
			
			ImGuiImplVulkan.SetCurrentContext(Context);

			if(!ImGuiImplVulkan.Init(ref info)) {
				throw new PlatformException("Could not initialize the Vulkan backend for ImGui");
			}
		}

		public override void Begin() {
			ImGuiImplGLFW.SetCurrentContext(Context);
			ImGuiImplVulkan.SetCurrentContext(Context);
			HImGui.SetCurrentContext(Context);
			
			ImGuiImplGLFW.NewFrame();
			ImGuiImplVulkan.NewFrame();
			HImGui.NewFrame();
		}
		
		public override void End() {
			ImGuiImplGLFW.SetCurrentContext(Context);
			ImGuiImplVulkan.SetCurrentContext(Context);
			HImGui.SetCurrentContext(Context);
			
			HImGui.Render();
			
			ImGuiImplVulkan.RenderDrawData(
				HImGui.GetDrawData(),
				new(_queue.CommandBuffer.Handle),
				default
			);
			
			if((IO.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
				HImGui.UpdatePlatformWindows();
				HImGui.RenderPlatformWindowsDefault();
			}
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
			
			// ImGuiImplGLFW.SetCurrentContext(Context);
			// ImGuiImplVulkan.SetCurrentContext(Context);
			// HImGui.SetCurrentContext(Context);
			
			// ImGuiImplVulkan.Shutdown();
			// ImGuiImplGLFW.Shutdown();
			// HImGui.DestroyContext(Context);
		}
	}
}
