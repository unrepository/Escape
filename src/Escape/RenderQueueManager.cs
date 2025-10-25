using System.Diagnostics;
using Escape.Renderer;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Vulkan;
using Silk.NET.Vulkan;
using Framebuffer = Escape.Renderer.Framebuffer;

namespace Escape {
	
	public static class RenderQueueManager {
		
		public static Dictionary<string, RenderQueue> Queues { get; } = [];

		public static RenderQueue Create(
			IPlatform platform,
			string id,
			Framebuffer? renderTarget = null,
			RenderQueue.Family family = RenderQueue.Family.Graphics,
			RenderQueue.Format format = RenderQueue.Format.R8G8B8A8Srgb
		) {
			RenderQueue queue;

			switch(platform) {
				case VkPlatform vkPlatform:
					var vkQueue = new VkRenderQueue(vkPlatform, family, format);
					
					// attachments
					vkQueue.CreateAttachment(Framebuffer.AttachmentType.Color);
					vkQueue.CreateAttachment(Framebuffer.AttachmentType.Depth, Format.D32Sfloat); // TODO some method to figure out the format depending on the device
					
					vkQueue.CreateSubpass(
						[ Framebuffer.AttachmentType.Color, Framebuffer.AttachmentType.Depth ],
						new SubpassDependency {
							SrcSubpass = Vk.SubpassExternal,
							DstSubpass = 0,
							SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
							SrcAccessMask = 0,
							DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
							DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit
						}
					);
					
					vkQueue.RenderTarget = renderTarget;
					vkQueue.Initialize();

					queue = vkQueue;
					break;
				case GLPlatform glPlatform:
					var glQueue = new GLRenderQueue(glPlatform, family, format);
					glQueue.RenderTarget = renderTarget;
					glQueue.Initialize();
					
					queue = glQueue;
					break;
				default:
					throw new NotImplementedException("PlatformImpl");
			}

			Queues[id] = queue;
			return queue;
		}

		public static void Add(string id, RenderQueue queue) {
			Queues[id] = queue;
		}

		public static RenderQueue? Get(string id) {
			if(!Queues.TryGetValue(id, out var queue)) {
				return queue;
			}

			return null;
		}
	}
}
