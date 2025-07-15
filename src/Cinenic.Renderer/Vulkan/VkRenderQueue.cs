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

namespace Cinenic.Renderer.Vulkan {
	
	public class VkRenderQueue : RenderQueue {

		public const int MAX_FRAMES_IN_FLIGHT = 2;
		
		public RenderPass Base;

		public new VkFramebuffer RenderTarget {
			get;
			set {
				_logger.Debug("Creating synchronization objects");
			
				// synchronization objects
				var semaphoreInfo = new SemaphoreCreateInfo {
					SType = StructureType.SemaphoreCreateInfo
				};
			
				var fenceInfo = new FenceCreateInfo {
					SType = StructureType.FenceCreateInfo,
					Flags = FenceCreateFlags.SignaledBit
				};

				Result result;

				unsafe {
					for(int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++) {
						if(
							(result = _platform.API.CreateSemaphore(
								_platform.PrimaryDevice!.Logical,
								semaphoreInfo,
								null,
								out var imageAvailable
							)) != Result.Success
						) {
							throw new PlatformException($"Could not create the image available semaphore: {result}");
						}
				
						if(
							(result = _platform.API.CreateFence(
								_platform.PrimaryDevice!.Logical,
								fenceInfo,
								null,
								out var inFlight
							)) != Result.Success
						) {
							throw new PlatformException($"Could not create the in-flight semaphore: {result}");
						}
				
						_imagesAvailable.Add(imageAvailable);
						_inFlightFrames.Add(inFlight);
					}
					
					for(int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++) {
						if(
							(result = _platform.API.CreateSemaphore(
								_platform.PrimaryDevice!.Logical,
								semaphoreInfo,
								null,
								out var renderComplete
							)) != Result.Success
						) {
							throw new PlatformException($"Could not create the render complete semaphore: {result}");
						}
				
						_rendersComplete.Add(renderComplete);
					}
				}
				
				// create command buffers
				CommandBuffers = new CommandBuffer[MAX_FRAMES_IN_FLIGHT];

				unsafe {
					var bufferAllocateInfo = new CommandBufferAllocateInfo {
						SType = StructureType.CommandBufferAllocateInfo,
						CommandPool = CommandPool.Value,
						Level = CommandBufferLevel.Primary,
						CommandBufferCount = (uint) CommandBuffers.Length
					};

					fixed(CommandBuffer* commandBuffersPtr = CommandBuffers) {
						if(
							(result = _platform.API.AllocateCommandBuffers(
								_platform.PrimaryDevice.Logical,
								bufferAllocateInfo,
								commandBuffersPtr
							)) != Result.Success
						) {
							throw new PlatformException($"Could not allocate command buffers: {result}");
						}
					}
				}

				CurrentFrame = 0;
				field = value;
			}
		}

		public List<AttachmentDescription> Attachments { get; } = [];
		public List<SubpassDescription> Subpasses { get; } = [];
		public List<SubpassDependency> SubpassDependencies { get; } = [];

		public CommandPool? CommandPool { get; private set; }
		public CommandBuffer[] CommandBuffers { get; private set; } = [];
		public CommandBuffer CommandBuffer => CommandBuffers[CurrentFrame];

		public int CurrentImage { get; private set; } = 0;
		public int CurrentFrame { get; private set; } = 0;
		
		internal readonly VkColorFormat VkColorFormat;
		internal readonly VkColorSpace VkColorSpace;
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly VkPlatform _platform;
		
		private readonly List<Semaphore> _imagesAvailable = [];
		private readonly List<Semaphore> _rendersComplete = [];
		private readonly List<Fence> _inFlightFrames = [];
		private Fence[] _inFlightImages = [];
		
		// public VkRenderQueue(
		// 	VkPlatform platform, Family family, Format format,
		// 	Window window
		// ) : this(platform, family, format) {
		// 	Viewport = new Vector4D<int>(0, 0, (int) window.Width, (int) window.Height);
		// 	Scissor = new Vector4D<int>(0, 0, (int) window.Width, (int) window.Height);
		// 	RenderTarget = (VkFramebuffer) window.Framebuffer;
		// }
		//
		// public VkRenderQueue(
		// 	VkPlatform platform, Family family, Format format,
		// 	Vector4D<int> viewport, Vector4D<int> scissor,
		// 	Framebuffer renderTarget
		// ) : this(platform, family, format) {
		// 	Viewport = viewport;
		// 	Scissor = scissor;
		// 	RenderTarget = (VkFramebuffer) renderTarget;
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
		}
		
		public unsafe override void Initialize() {
			var attachments = Attachments.ToArray();
			var subpasses = Subpasses.ToArray();
			var dependencies = SubpassDependencies.ToArray();
			
			Result result;
			
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
				
				if(
					(result = _platform.API.CreateRenderPass(_platform.PrimaryDevice!.Logical, renderPassInfo, null, out var pass))
					!= Result.Success
				) {
					throw new PlatformException($"Could not create render pass: {result}");
				}
				
				Base = pass;
			}
			
			_logger.Debug("Initialized render pass");

			var commandPoolInfo = new CommandPoolCreateInfo {
				SType = StructureType.CommandPoolCreateInfo,
				Flags = CommandPoolCreateFlags.ResetCommandBufferBit
			};

			commandPoolInfo.QueueFamilyIndex = Type switch {
				Family.Graphics => (uint) _platform.PrimaryDevice!.GraphicsFamily,
				Family.Compute => (uint) _platform.PrimaryDevice!.ComputeFamily,
				_ => throw new NotImplementedException()
			};

			//CommandPool commandPool;

			if(
				(result = _platform.API.CreateCommandPool(
					_platform.PrimaryDevice!.Logical,
					commandPoolInfo,
					null,
					out var commandPool
				)) != Result.Success
			) {
				throw new PlatformException($"Could not create command pool: {result}");
			}

			CommandPool = commandPool;
			_logger.Debug("Created command pool");
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

		public unsafe override void Begin(Framebuffer renderTarget) {
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
			//_logger.Warn(CurrentImage);
			//_logger.Warn(CurrentFrame);
			VkExtension
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

			Result result;
			if((result = _platform.API.BeginCommandBuffer(CommandBuffer, &cmdBeginInfo)) != Result.Success) {
				throw new PlatformException($"Could not begin the command buffer: {result}");
			}
			
			// dumb shit image transition?
			/*var memoryBarrier = new ImageMemoryBarrier {
				SType = StructureType.ImageMemoryBarrier,
				OldLayout = ImageLayout.PresentSrcKhr,
				NewLayout = ImageLayout.ColorAttachmentOptimal,
				SrcQueueFamilyIndex = 0,
				DstQueueFamilyIndex = 0,
				Image = vkRenderTarget.SwapchainImages[CurrentFrame],
				SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
				SrcAccessMask = AccessFlags.ColorAttachmentWriteBit,
				DstAccessMask = AccessFlags.MemoryReadBit
			};
			
			_platform.API.CmdPipelineBarrier(
				CommandBuffer,
				PipelineStageFlags.ColorAttachmentOutputBit,
				PipelineStageFlags.BottomOfPipeBit,
				0,
				0, null,
				0, null,
				1, &memoryBarrier
			);*/
			
			var passBeginInfo = new RenderPassBeginInfo {
				SType = StructureType.RenderPassBeginInfo,
				RenderPass = Base,
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
		}
		
		public unsafe override void End(Framebuffer renderTarget) {
			Debug.Assert(renderTarget is VkFramebuffer);
			Debug.Assert(CommandPool.HasValue);
			Debug.Assert(CommandBuffers.Length > 0);

			var vkRenderTarget = (VkFramebuffer) renderTarget;
			
			// end render pass & command buffer
			_platform.API.CmdEndRenderPass(CommandBuffer);
			
			// dumb shit image transition?
			// var memoryBarrier = new ImageMemoryBarrier {
			// 	SType = StructureType.ImageMemoryBarrier,
			// 	OldLayout = ImageLayout.ColorAttachmentOptimal,
			// 	NewLayout = ImageLayout.PresentSrcKhr,
			// 	SrcQueueFamilyIndex = 0,
			// 	DstQueueFamilyIndex = 0,
			// 	Image = vkRenderTarget.SwapchainImages[CurrentImage],
			// 	SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
			// 	SrcAccessMask = AccessFlags.ColorAttachmentWriteBit,
			// 	DstAccessMask = AccessFlags.MemoryReadBit
			// };
			//
			// _platform.API.CmdPipelineBarrier(
			// 	CommandBuffer,
			// 	PipelineStageFlags.ColorAttachmentOutputBit,
			// 	PipelineStageFlags.BottomOfPipeBit,
			// 	0,
			// 	0, null,
			// 	0, null,
			// 	1, &memoryBarrier
			// );

			Result result;
			if((result = _platform.API.EndCommandBuffer(CommandBuffer)) != Result.Success) {
				throw new PlatformException($"Could not end the command buffer: {result}");
			}
			
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
			
			if(
				(result = _platform.API.QueueSubmit(
					_platform.PrimaryDevice!.GraphicsQueue,
					1,
					submitInfo,
					_inFlightFrames[CurrentFrame]
				)) != Result.Success
			) {
				throw new PlatformException($"Could not submit graphics queue: {result}");
			}

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
				
			VkExtension
				.Get<KhrSwapchain>(_platform, _platform.PrimaryDevice)
				.QueuePresent(_platform.PrimaryDevice.SurfaceQueue.Value, presentInfo);

			CurrentFrame = (CurrentFrame + 1) % MAX_FRAMES_IN_FLIGHT;
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);

			unsafe {
				var device = _platform.PrimaryDevice!.Logical;
				
				_platform.API.DestroyRenderPass(device, Base, null);
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
