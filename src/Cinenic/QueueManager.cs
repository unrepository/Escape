using System.Diagnostics;
using Cinenic.Renderer;
using Cinenic.Renderer.Vulkan;
using Silk.NET.Vulkan;
using Framebuffer = Cinenic.Renderer.Framebuffer;

namespace Cinenic {
	
	public static class QueueManager {
		
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
					vkQueue.CreateAttachment();
					vkQueue.CreateSubpass(
						0,
						ImageLayout.ColorAttachmentOptimal,
						new SubpassDescription {
							ColorAttachmentCount = 1
						},
						new SubpassDependency {
							SrcSubpass = Vk.SubpassExternal,
							DstSubpass = 0,
							SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
							SrcAccessMask = 0,
							DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
							DstAccessMask = AccessFlags.ColorAttachmentWriteBit
						}
					);
					vkQueue.RenderTarget = renderTarget;
					vkQueue.Initialize();

					queue = vkQueue;
					break;
				default:
					throw new NotImplementedException("PlatformImpl");
			}

			Queues[id] = queue;
			return queue;
		}

		public static RenderQueue? Get(string id) {
			if(!Queues.TryGetValue(id, out var queue)) {
				return queue;
			}

			return null;
		}
	}
}
