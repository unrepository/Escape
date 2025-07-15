using System.Diagnostics;
using System.Runtime.InteropServices;
using NLog;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using VkColorFormat = Silk.NET.Vulkan.Format;
using VkColorSpace = Silk.NET.Vulkan.ColorSpaceKHR;

using static Cinenic.Renderer.Vulkan.VkHelpers;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkRenderQueue : RenderQueue {

		public const int MAX_FRAMES_IN_FLIGHT = 2;
		
		public RenderPass RenderPass { get; private set; }
		
		public List<AttachmentDescription> Attachments { get; init; } = [];
		public List<SubpassDescription> Subpasses { get; init; } = [];
		public List<SubpassDependency> SubpassDependencies { get; init; } = [];

		public CommandPool? CommandPool { get; private set; }
		public CommandBuffer[] CommandBuffers { get; private set; } = [];
		public CommandBuffer CommandBuffer => CommandBuffers[CurrentFrame];

		public int CurrentImage { get; private set; } = 0;
		public int CurrentFrame { get; private set; } = 0;
		
		internal readonly VkColorFormat VkColorFormat;
		internal readonly VkColorSpace VkColorSpace;
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly VkPlatform _platform;
		
		private Semaphore[] _imagesAvailable;
		private Semaphore[] _rendersComplete;
		private Fence[] _inFlightFrames;
		
		// public VkRenderQueue(
		// 	VkPlatform platform, Family family, Format format,
		// 	Window window
		// ) : this(platform, family, format) {
		// 	Viewport = new Vector4D<int>(0, 0, (int) window.Width, (int) window.Height);
		// 	Scissor = new Vector4D<int>(0, 0, (int) window.Width, (int) window.Height);
		// }
		//
		// public VkRenderQueue(
		// 	VkPlatform platform, Family family, Format format,
		// 	Vector4D<int> viewport, Vector4D<int> scissor
		// ) : this(platform, family, format) {
		// 	Viewport = viewport;
		// 	Scissor = scissor;
		// }

		public unsafe VkRenderQueue(VkPlatform platform, Family family, Format format)
			: base(platform, family, format)
		{
			_platform = platform;

			switch(format) {
				case Format.R8G8B8A8Srgb:
					VkColorFormat = VkColorFormat.R8G8B8A8Srgb;
					VkColorSpace = VkColorSpace.SpaceSrgbNonlinearKhr;
					break;
			}
			
			_logger.Debug("Creating synchronization objects");
			
			// synchronization objects
			var semaphoreInfo = new SemaphoreCreateInfo {
				SType = StructureType.SemaphoreCreateInfo
			};
		
			var fenceInfo = new FenceCreateInfo {
				SType = StructureType.FenceCreateInfo,
				Flags = FenceCreateFlags.SignaledBit
			};

			_imagesAvailable = new Semaphore[MAX_FRAMES_IN_FLIGHT];
			_rendersComplete = new Semaphore[MAX_FRAMES_IN_FLIGHT];
			_inFlightFrames = new Fence[MAX_FRAMES_IN_FLIGHT];
			
			for(int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++) {
				VkCheck(
					_platform.API.CreateSemaphore(
						_platform.PrimaryDevice!.Logical,
						semaphoreInfo,
						null,
						out var imageAvailable
					),
					"Could not create the image available semaphore"
				);

				VkCheck(
					_platform.API.CreateSemaphore(
						_platform.PrimaryDevice!.Logical,
						semaphoreInfo,
						null,
						out var renderComplete
					),
					"Could not create the render complete semaphore"
				);
				
				VkCheck(
					_platform.API.CreateFence(
						_platform.PrimaryDevice!.Logical,
						fenceInfo,
						null,
						out var inFlight
					),
					"Could not create the in-flight fence"
				);
		
				_imagesAvailable[i] = imageAvailable;
				_rendersComplete[i] = renderComplete;
				_inFlightFrames[i] = inFlight;
			}
			
			// create command pool
			var commandPoolInfo = new CommandPoolCreateInfo {
				SType = StructureType.CommandPoolCreateInfo,
				Flags = CommandPoolCreateFlags.ResetCommandBufferBit
			};

			commandPoolInfo.QueueFamilyIndex = Type switch {
				Family.Graphics => (uint) _platform.PrimaryDevice!.GraphicsFamily,
				Family.Compute => (uint) _platform.PrimaryDevice!.ComputeFamily,
				_ => throw new NotImplementedException()
			};

			VkCheck(
				_platform.API.CreateCommandPool(
					_platform.PrimaryDevice!.Logical,
					commandPoolInfo,
					null,
					out var commandPool
				),
				"Could not create the command pool"
			);

			CommandPool = commandPool;
			_logger.Debug("Created command pool");
			
			// create command buffers
			CommandBuffers = new CommandBuffer[MAX_FRAMES_IN_FLIGHT];

			var bufferAllocateInfo = new CommandBufferAllocateInfo {
				SType = StructureType.CommandBufferAllocateInfo,
				CommandPool = CommandPool.Value,
				Level = CommandBufferLevel.Primary,
				CommandBufferCount = (uint) CommandBuffers.Length
			};

			fixed(CommandBuffer* commandBuffersPtr = CommandBuffers) {
				VkCheck(
					_platform.API.AllocateCommandBuffers(
						_platform.PrimaryDevice.Logical,
						bufferAllocateInfo,
						commandBuffersPtr
					),
					$"Could not allocate {bufferAllocateInfo.CommandBufferCount} command buffers"
				);
			}
		}
		
		public unsafe override void Initialize() {
			var attachments = Attachments.ToArray();
			var subpasses = Subpasses.ToArray();
			var dependencies = SubpassDependencies.ToArray();
			
			var renderPassInfo = new RenderPassCreateInfo {
				SType = StructureType.RenderPassCreateInfo,
				AttachmentCount = (uint) attachments.Length,
				SubpassCount = (uint) subpasses.Length,
				DependencyCount = (uint) dependencies.Length,
			};

			fixed(
				void* attachmentsPtr = attachments,
				subpassesPtr = subpasses,
				dependenciesPtr = dependencies
			) {
				renderPassInfo.PAttachments = (AttachmentDescription*) attachmentsPtr;
				renderPassInfo.PSubpasses = (SubpassDescription*) subpassesPtr;
				renderPassInfo.PDependencies = (SubpassDependency*) dependenciesPtr;

				VkCheck(
					_platform.API.CreateRenderPass(
						_platform.PrimaryDevice!.Logical,
						renderPassInfo,
						null,
						out var pass
					),
					"Could not create the render pass"
				);
				
				RenderPass = pass;
			}
			
			_logger.Debug("Initialized render pass");
		}

		public void CreateAttachment(VkColorFormat? format = null) {
			var attachment = new AttachmentDescription {
				Format = format ?? VkColorFormat,
				Samples = SampleCountFlags.Count1Bit,
				LoadOp = AttachmentLoadOp.Clear,
				StoreOp = AttachmentStoreOp.Store,
				StencilLoadOp = AttachmentLoadOp.DontCare,
				StencilStoreOp = AttachmentStoreOp.DontCare,
				InitialLayout = ImageLayout.Undefined,
				FinalLayout = ImageLayout.PresentSrcKhr
			};
			
			Attachments.Add(attachment);
			_logger.Debug("Created attachment for {RenderPass}", this);
		}

		public unsafe void CreateSubpass(
			uint attachmentIndex, ImageLayout layout,
			SubpassDescription description, SubpassDependency? dependency = null)
		{
			description.PipelineBindPoint = Type switch {
				Family.Graphics => PipelineBindPoint.Graphics,
				Family.Compute => PipelineBindPoint.Compute,
				_ => throw new NotImplementedException()
			};
			
			var attachmentReference = new AttachmentReference {
				Attachment = attachmentIndex,
				Layout = layout
			};

			var handle = GCHandle.Alloc(attachmentReference, GCHandleType.Pinned);
			
			description.PColorAttachments = (AttachmentReference*) handle.AddrOfPinnedObject();
			description.PInputAttachments = (AttachmentReference*) handle.AddrOfPinnedObject();
			
			Subpasses.Add(description);
			if(dependency.HasValue) SubpassDependencies.Add(dependency.Value);
			
			_logger.Debug("Created subpass for {RenderPass}, attachmentIndex={AttachmentIndex}, layout={Layout}, bindPoint={BindPoint}",
				this, attachmentIndex, layout, description.PipelineBindPoint);
		}

		public unsafe override bool Begin(Framebuffer renderTarget) {
			Debug.Assert(renderTarget is VkFramebuffer);
			Debug.Assert(CommandPool.HasValue);
			Debug.Assert(CommandBuffers.Length > 0);

			var vkRenderTarget = (VkFramebuffer) renderTarget;

			// synchronization wait
			_platform.API.WaitForFences(
				_platform.PrimaryDevice!.Logical,
				1,
				_inFlightFrames[CurrentFrame],
				true,
				ulong.MaxValue
			);
			
			// acquire frame
			uint nextImage = 0;
			var result = VkExtension
				.Get<KhrSwapchain>(_platform, _platform.PrimaryDevice)
				.AcquireNextImage(
					_platform.PrimaryDevice.Logical,
					vkRenderTarget.Swapchain,
					ulong.MaxValue,
					_imagesAvailable[CurrentFrame],
					default,
					ref nextImage
				);
			CurrentImage = (int) nextImage;

			if(result == Result.ErrorOutOfDateKhr) {
				return false;
			}
			
			if(result != Result.Success && result != Result.SuboptimalKhr) {
				throw new PlatformException($"Failed to acquire swapchain image: {result}");
			}
			
			// reset synchronization
			_platform.API.ResetFences(_platform.PrimaryDevice.Logical, 1, _inFlightFrames[CurrentFrame]);
			
			// reset command buffer
			_platform.API.ResetCommandBuffer(CommandBuffer, 0);
			
			// begin command buffer recording and render pass
			var cmdBeginInfo = new CommandBufferBeginInfo {
				SType = StructureType.CommandBufferBeginInfo,
				Flags = 0,
				PInheritanceInfo = null
			};

			VkCheck(
				_platform.API.BeginCommandBuffer(CommandBuffer, &cmdBeginInfo),
				"Could not begin the command buffer"
			);
			
			var passBeginInfo = new RenderPassBeginInfo {
				SType = StructureType.RenderPassBeginInfo,
				RenderPass = RenderPass,
				Framebuffer = vkRenderTarget.SwapchainFramebuffers[CurrentImage],
				RenderArea = {
					Offset = { X = Viewport.X, Y = Viewport.Y },
					Extent = { Width = (uint) Viewport.Z, Height = (uint) Viewport.W }
				},
				ClearValueCount = 1
			};
			
			var clearColor = new ClearValue { Color = new ClearColorValue { Float32_0 = 0, Float32_1 = 0, Float32_2 = 0, Float32_3 = 0 } };
			passBeginInfo.PClearValues = &clearColor;
			
			_platform.API.CmdBeginRenderPass(CommandBuffer, &passBeginInfo, SubpassContents.Inline);
			return true;
		}
		
		public unsafe override bool End(Framebuffer renderTarget) {
			Debug.Assert(renderTarget is VkFramebuffer);
			Debug.Assert(CommandPool.HasValue);
			Debug.Assert(CommandBuffers.Length > 0);

			var vkRenderTarget = (VkFramebuffer) renderTarget;
			
			// end render pass & command buffer
			_platform.API.CmdEndRenderPass(CommandBuffer);

			VkCheck(_platform.API.EndCommandBuffer(CommandBuffer), "Could not end the command buffer");
			
			// synchronization
			var imageAvailable = _imagesAvailable[CurrentFrame];
			var renderComplete = _rendersComplete[CurrentFrame];
			
			var commandBuffer = CommandBuffer;
			// TODO AllCommandsBit/AllGraphicsBit test
			var waitDstStageMask = stackalloc PipelineStageFlags[1] { PipelineStageFlags.ColorAttachmentOutputBit };
				
			var submitInfo = new SubmitInfo {
				SType = StructureType.SubmitInfo,
				WaitSemaphoreCount = 1,
				PWaitSemaphores = &imageAvailable,
				PWaitDstStageMask = waitDstStageMask,
				SignalSemaphoreCount = 1,
				PSignalSemaphores = &renderComplete,
				CommandBufferCount = 1,
				PCommandBuffers = &commandBuffer
			};

			VkCheck(
				_platform.API.QueueSubmit(
					_platform.PrimaryDevice!.GraphicsQueue,
					1,
					submitInfo,
					_inFlightFrames[CurrentFrame]
				),
				"Could not submit the graphics queue"
			);

			var image = CurrentImage;
			
			// presentation
			var targetSwapchain = vkRenderTarget.Swapchain;
				
			var presentInfo = new PresentInfoKHR {
				SType = StructureType.PresentInfoKhr,
				WaitSemaphoreCount = 1,
				PWaitSemaphores = &renderComplete,
				SwapchainCount = 1,
				PSwapchains = &targetSwapchain,
				PImageIndices = (uint*) &image,
				PResults = null
			};

			var result = VkExtension
				.Get<KhrSwapchain>(_platform, _platform.PrimaryDevice)
				.QueuePresent(_platform.PrimaryDevice.SurfaceQueue.Value, presentInfo);

			if(result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr) {
				return false;
			}
			
			if(result != Result.Success) {
				throw new PlatformException($"Failed to present queue: {result}");
			}
			
			CurrentFrame = (CurrentFrame + 1) % MAX_FRAMES_IN_FLIGHT;
			return true;
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);

			unsafe {
				var device = _platform.PrimaryDevice!.Logical;
				
				_platform.API.DestroyRenderPass(device, RenderPass, null);
				_platform.API.DestroyCommandPool(device, CommandPool.Value, null);
				
				for(int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++) {
					_platform.API.DestroySemaphore(device, _imagesAvailable[i], null);
					_platform.API.DestroySemaphore(device, _rendersComplete[i], null);
					_platform.API.DestroyFence(device, _inFlightFrames[i], null);
				}
			}
		}
	}
}
