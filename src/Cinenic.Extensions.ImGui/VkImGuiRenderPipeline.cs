using System.Diagnostics;
using Cinenic.Renderer.Shader;
using Cinenic.Renderer.Vulkan;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.Vulkan;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using VkFramebuffer = Cinenic.Renderer.Vulkan.VkFramebuffer;
using HImGui = Hexa.NET.ImGui.ImGui;

namespace Cinenic.Extensions.ImGui {
	
	public class VkImGuiRenderPipeline : VkRenderPipeline {

		public required ImGuiController Controller { get; init; }
		
		private readonly new VkPlatform _platform;
		
		public VkImGuiRenderPipeline(VkPlatform platform, VkRenderQueue queue) : base(platform, queue) {
			_platform = platform;
		}

		// yes, this is the exact same think as in the regular VkRenderPipeline but without the pipeline (and program) binding
		public override bool Begin() {
			Debug.Assert(Queue is VkRenderQueue);
			Debug.Assert(Queue.RenderTarget is not null, "Queue.RenderTarget is null! Did you forget to assign it?");

			ImGuiImplGLFW.SetCurrentContext(Controller.Context);
			ImGuiImplVulkan.SetCurrentContext(Controller.Context);
			HImGui.SetCurrentContext(Controller.Context);
			
			ImGuiImplGLFW.NewFrame();
			ImGuiImplVulkan.NewFrame();
			HImGui.NewFrame();
			return true;
		}

		public override bool End() {
			ImGuiImplGLFW.SetCurrentContext(Controller.Context);
			ImGuiImplVulkan.SetCurrentContext(Controller.Context);
			HImGui.SetCurrentContext(Controller.Context);
			
			HImGui.Render();
			
			if(Queue.RenderTarget is VkWindow.WindowFramebuffer windowFramebuffer) {
				windowFramebuffer.Window.Base.DoUpdate();
				if(!windowFramebuffer.Window.Base.IsClosing) windowFramebuffer.Window.Base.DoEvents();
				if(windowFramebuffer.Window.Base.IsClosing) {
					Queue.RenderTarget.Dispose();
					Queue.RenderTarget = null;
					return false;
				}
				windowFramebuffer.Window.Base.MakeCurrent();
			}
			
			if(!Queue.Begin()) {
				RecreateFramebuffer(
					_platform,
					(VkFramebuffer) Queue.RenderTarget,
					out var newFramebuffer
				);
				
				RecreateQueue(
					_platform,
					(VkRenderQueue) Queue,
					out var newQueue
				);
				
				Queue = newQueue;
				Queue.RenderTarget = newFramebuffer;
			}

			var vkQueue = (VkRenderQueue) Queue;
			var vkRenderTarget = (VkFramebuffer) Queue.RenderTarget;
			
			var viewport = new Viewport {
				X = Queue.Viewport.X,
				Y = Queue.Viewport.Y,
				Width = Queue.Viewport.Z > 0 ? Queue.Viewport.Z : vkRenderTarget.Size.X,
				Height = Queue.Viewport.W > 0 ? Queue.Viewport.W : vkRenderTarget.Size.Y,
				MinDepth = 0,
				MaxDepth = 1
			};
			
			var scissor = new Rect2D {
				Offset = {
					X = Queue.Scissor.X,
					Y = Queue.Scissor.Y
				},
				Extent = {
					Width = (uint) (Queue.Scissor.Z > 0 ? Queue.Scissor.Z : viewport.Width),
					Height = (uint) (Queue.Scissor.W > 0 ? Queue.Scissor.W : viewport.Height)
				}
			};
			
			unsafe {
				_platform.API.CmdSetViewport(vkQueue.CommandBuffer, 0, 1, &viewport);
				_platform.API.CmdSetScissor(vkQueue.CommandBuffer, 0, 1, &scissor);
			}

			var pipeline = (VkRenderPipeline) Controller.Pipeline;
			var queue = (VkRenderQueue) Controller.Queue;
			
			ImGuiImplVulkan.RenderDrawData(
				HImGui.GetDrawData(),
				new(queue.CommandBuffer.Handle),
				default
			);
			
			if((Controller.IO.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0) {
				HImGui.UpdatePlatformWindows();
				HImGui.RenderPlatformWindowsDefault();
			}

			return base.End();
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			Queue.Dispose();
		}
	}
}
