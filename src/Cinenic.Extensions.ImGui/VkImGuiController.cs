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
		
		public unsafe VkImGuiController(VkPlatform platform) : base(platform) {
			_platform = platform;
			//_queue = (VkRenderQueue) queue;
			
			//Debug.Assert(_pipeline.Queue.RenderTarget is not null, "Render queue must have a target set!");
			//Debug.Assert(_pipeline.Queue.RenderTarget == window.Framebuffer, "Window must belong to the render queue!");

			Context = HImGui.CreateContext();
			
			// _queue = (VkRenderQueue) RenderQueueManager.Create(platform, $"imgui_{GetHashCode()}");
			// Queue = _queue;
			//
			// _pipeline = new VkImGuiRenderPipeline(platform, _queue) {
			// 	Controller = this
			// };
			//
			// Pipeline = _pipeline;
			// RenderPipelineManager.Add($"imgui_{GetHashCode()}", _pipeline);
		}

		public unsafe void Initialize(Window window, RenderQueue queue) {
			//Queue.RenderTarget = window.Framebuffer;
			
			HImGui.SetCurrentContext(Context);

			IO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
			IO.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
			IO.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

			HImGui.StyleColorsDark();
			
			ImGuiImplGLFW.SetCurrentContext(Context);

			if(!ImGuiImplGLFW.InitForVulkan(new GLFWwindowPtr((GLFWwindow*) window.Base.Handle), true)) {
				throw new PlatformException("Could not initialize the GLFW backend for ImGui");
			}

			// var queue = new VkImGuiRenderQueue(platform, RenderQueue.Family.Graphics, RenderQueue.Format.R8G8B8A8Srgb) {
			// 	Controller = this
			// };
			//
			// queue.CreateAttachment(Framebuffer.AttachmentType.Color);
			// queue.CreateSubpass(
			// 	[ Framebuffer.AttachmentType.Color ],
			// 	new SubpassDependency {
			// 		SrcSubpass = Vk.SubpassExternal,
			// 		DstSubpass = 0,
			// 		SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
			// 		SrcAccessMask = 0,
			// 		DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
			// 		DstAccessMask = AccessFlags.ColorAttachmentWriteBit
			// 	}
			// );
			//
			// queue.RenderTarget = window.Framebuffer;
			// queue.Initialize();
			//
			// Queue = queue;
			// RenderQueueManager.Add($"imgui_{GetHashCode()}", queue);

			var vkQueue = (VkRenderQueue) queue;
			
			var info = new ImGuiImplVulkanInitInfo {
				ApiVersion = Vk.Version10,
				Instance = new(_platform.Vk.Handle),
				PhysicalDevice = new(_platform.PrimaryDevice.Physical.Handle),
				Device = new(_platform.PrimaryDevice.Logical.Handle),
				QueueFamily = (uint) _platform.PrimaryDevice.GraphicsFamily,
				Queue = new(_platform.PrimaryDevice.GraphicsQueue.Handle),
				DescriptorPoolSize = 16,
				RenderPass = new((nint) vkQueue.Base.Handle),
				MinImageCount = 2,
				ImageCount = (uint) vkQueue.CommandBuffers.Length,
				MSAASamples = 1,
				Subpass = 0,
			};
			
			ImGuiImplVulkan.SetCurrentContext(Context);

			if(!ImGuiImplVulkan.Init(ref info)) {
				throw new PlatformException("Could not initialize the Vulkan backend for ImGui");
			}
		}

		public void Begin() {
			ImGuiImplGLFW.SetCurrentContext(Context);
			ImGuiImplVulkan.SetCurrentContext(Context);
			HImGui.SetCurrentContext(Context);
			
			ImGuiImplGLFW.NewFrame();
			ImGuiImplVulkan.NewFrame();
			HImGui.NewFrame();
		}
		
		public void End(RenderQueue queue) {
			ImGuiImplGLFW.SetCurrentContext(Context);
			ImGuiImplVulkan.SetCurrentContext(Context);
			HImGui.SetCurrentContext(Context);
			
			HImGui.Render();
			
			ImGuiImplVulkan.RenderDrawData(
				HImGui.GetDrawData(),
				new(((VkRenderQueue) queue).CommandBuffer.Handle),
				default
			);
			
			if((IO.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
				HImGui.UpdatePlatformWindows();
				HImGui.RenderPlatformWindowsDefault();
			}
		}
		
		public override void Dispose() {
			// ImGuiImplGLFW.SetCurrentContext(Context);
			// ImGuiImplVulkan.SetCurrentContext(Context);
			// HImGui.SetCurrentContext(Context);
			//
			// // ImGuiImplVulkan.Shutdown();
			// // ImGuiImplGLFW.Shutdown();
			// HImGui.DestroyContext(Context);
		}
	}
}
