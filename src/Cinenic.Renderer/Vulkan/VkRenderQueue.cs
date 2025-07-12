using System.Diagnostics;
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
		
		public RenderPass Base;

		/*public override Vector4D<int> Viewport {
			get => field;
			set {
				field = value;
				if(!CommandBuffer.HasValue) return;
				
				var viewport = new Viewport {
					X = value.X,
					Y = value.Y,
					Width = value.Z,
					Height = value.W,
					MinDepth = 0,
					MaxDepth = 1
				};

				unsafe {
					_platform.API.CmdSetViewport(CommandBuffer.Value, 0, 1, &viewport);
				}
			}
		}
		
		public override Vector4D<int> Scissor {
			get => field;
			set {
				field = value;
				if(!CommandBuffer.HasValue) return;

				var scissor = new Rect2D {
					Offset = { X = value.X, Y = value.Y },
					Extent = { Width = (uint) value.Z, Height = (uint) value.W }
				};

				unsafe {
					_platform.API.CmdSetScissor(CommandBuffer.Value, 0, 1, &scissor);
				}
			}
		}*/

		public List<AttachmentDescription> Attachments { get; } = [];
		public List<SubpassDescription> Subpasses { get; } = [];
		public List<SubpassDependency> SubpassDependencies { get; } = [];

		public CommandPool? CommandPool { get; private set; }
		public CommandBuffer? CommandBuffer { get; private set; }
			
		internal readonly VkColorFormat VkColorFormat;
		internal readonly VkColorSpace VkColorSpace;
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly VkPlatform _platform;
		
		private readonly Semaphore _imageAvailable;
		private readonly Semaphore _renderComplete;
		private readonly Fence _inFlight;

		private uint _imageIndex;

		public unsafe VkRenderQueue(VkPlatform platform, Family family, Format format)
			: base(platform, family, format)
		{
			_platform = platform;

			switch(format) {
				case Format.R8G8B8A8Srgb:
					VkColorFormat = VkColorFormat.R8G8B8Srgb;
					VkColorSpace = VkColorSpace.SpaceSrgbNonlinearKhr;
					break;
			}
			
			_logger.Debug("Creating synchronization objects");
			
			// TODO just realized Silk.NET stuff sets SType automatically
			var semaphoreInfo = new SemaphoreCreateInfo { };

			Result result;
			if(
				(result = platform.API.CreateSemaphore(
					platform.PrimaryDevice!.Logical,
					&semaphoreInfo,
					null,
					out _imageAvailable
				)) != Result.Success
			) {
				throw new PlatformException($"Could not create the image available semaphore: {result}");
			}
			
			if(
				(result = platform.API.CreateSemaphore(
					platform.PrimaryDevice!.Logical,
					&semaphoreInfo,
					null,
					out _renderComplete
				)) != Result.Success
			) {
				throw new PlatformException($"Could not create the render complete semaphore: {result}");
			}
			
			var fenceInfo = new FenceCreateInfo {
				Flags = FenceCreateFlags.SignaledBit
			};
			
			if(
				(result = platform.API.CreateFence(
					platform.PrimaryDevice!.Logical,
					&fenceInfo,
					null,
					out _inFlight
				)) != Result.Success
			) {
				throw new PlatformException($"Could not create the in-flight semaphore: {result}");
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
				DependencyCount = (uint) dependencies.Length
			};

			fixed(AttachmentDescription* attachmentsPtr = attachments) {
				renderPassInfo.PAttachments = attachmentsPtr;
			}

			fixed(SubpassDescription* subpassesPtr = subpasses) {
				renderPassInfo.PSubpasses = subpassesPtr;
			}
			
			fixed(SubpassDependency* dependenciesPtr = dependencies) {
				renderPassInfo.PDependencies = dependenciesPtr;
			}

			if(
				(result = _platform.API.CreateRenderPass(_platform.PrimaryDevice!.Logical, &renderPassInfo, null, out var pass))
				!= Result.Success
			) {
				throw new PlatformException($"Could not create render pass: {result}");
			}

			Base = pass;
			
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

			CommandPool commandPool;

			if(
				(result = _platform.API.CreateCommandPool(
					_platform.PrimaryDevice!.Logical,
					&commandPoolInfo,
					null,
					&commandPool
				)) != Result.Success
			) {
				throw new PlatformException($"Could not create command pool: {result}");
			}

			CommandPool = commandPool;
			_logger.Debug("Created command pool");

			var bufferAllocateInfo = new CommandBufferAllocateInfo {
				SType = StructureType.CommandBufferAllocateInfo,
				CommandPool = CommandPool.Value,
				Level = CommandBufferLevel.Primary,
				CommandBufferCount = 1
			};
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
			var attachmentReference = new AttachmentReference {
				Attachment = attachmentIndex,
				Layout = layout
			};

			description.PipelineBindPoint = Type switch {
				Family.Graphics => PipelineBindPoint.Graphics,
				Family.Compute => PipelineBindPoint.Compute,
				_ => throw new NotImplementedException()
			};
			
			description.PColorAttachments = &attachmentReference;
			description.PInputAttachments = &attachmentReference;
			
			Subpasses.Add(description);
			if(dependency.HasValue) SubpassDependencies.Add(dependency.Value);
			
			_logger.Debug("Created subpass for {RenderPass}, attachmentIndex={AttachmentIndex}, layout={Layout}, bindPoint={BindPoint}",
				this, attachmentIndex, layout, description.PipelineBindPoint);
		}

		public unsafe override void Begin(Framebuffer renderTarget) {
			Debug.Assert(renderTarget is VkFramebuffer);
			Debug.Assert(CommandPool.HasValue);
			Debug.Assert(CommandBuffer.HasValue);

			var vkRenderTarget = (VkFramebuffer) renderTarget;

			// synchronization wait
			fixed(Fence* fence = &_inFlight) {
				_platform.API.WaitForFences(
					_platform.PrimaryDevice!.Logical,
					1,
					fence,
					true,
					ulong.MaxValue
				);

				_platform.API.ResetFences(_platform.PrimaryDevice.Logical, 1, fence);
			}
			
			// acquire frame
			VkExtension.Get<KhrSwapchain>(_platform, _platform.PrimaryDevice)
					   .AcquireNextImage(
						   _platform.PrimaryDevice.Logical,
						   vkRenderTarget.Swapchain,
						   ulong.MaxValue,
						   _imageAvailable,
						   new Fence(),
						   ref _imageIndex
					   );

			// reset command buffer
			_platform.API.ResetCommandBuffer(CommandBuffer.Value, 0);
			
			// begin command buffer recording and render pass
			var cmdBeginInfo = new CommandBufferBeginInfo {
				SType = StructureType.CommandBufferBeginInfo,
				Flags = 0,
				PInheritanceInfo = null
			};

			Result result;
			if((result = _platform.API.BeginCommandBuffer(CommandBuffer.Value, &cmdBeginInfo)) != Result.Success) {
				throw new PlatformException($"Could not begin the command buffer: {result}");
			}
			
			var passBeginInfo = new RenderPassBeginInfo {
				SType = StructureType.RenderPassBeginInfo,
				RenderPass = Base,
				Framebuffer = vkRenderTarget.Base,
				RenderArea = {
					Offset = { X = Viewport.X, Y = Viewport.Y },
					Extent = { Width = (uint) Viewport.Z, Height = (uint) Viewport.W }
				},
				ClearValueCount = 1
			};

			passBeginInfo.PClearValues[0] = new ClearValue(
				new ClearColorValue(0, 0, 0, 0),
				new ClearDepthStencilValue(0, 0)
			);
			
			_platform.API.CmdBeginRenderPass(CommandBuffer.Value, &passBeginInfo, SubpassContents.Inline);
		}
		
		public unsafe override void End(Framebuffer renderTarget) {
			Debug.Assert(renderTarget is VkFramebuffer);
			Debug.Assert(CommandPool.HasValue);
			Debug.Assert(CommandBuffer.HasValue);

			var vkRenderTarget = (VkFramebuffer) renderTarget;
			
			// end render pass & command buffer
			_platform.API.CmdEndRenderPass(CommandBuffer!.Value);

			Result result;
			if((result = _platform.API.EndCommandBuffer(CommandBuffer.Value)) != Result.Success) {
				throw new PlatformException($"Could not end the command buffer: {result}");
			}
			
			// synchronization
			fixed(Semaphore* imageAvailablePtr = &_imageAvailable) {
				fixed(Semaphore* renderCompletePtr = &_renderComplete) {
					var submitInfo = new SubmitInfo {
						WaitSemaphoreCount = 1,
						PWaitSemaphores = imageAvailablePtr,
						SignalSemaphoreCount = 1,
						PSignalSemaphores = renderCompletePtr
					};

					// TODO AllCommandsBit/AllGraphicsBit test
					submitInfo.PWaitDstStageMask[0] = PipelineStageFlags.ColorAttachmentOutputBit;

					if(
						(result = _platform.API.QueueSubmit(
							_platform.PrimaryDevice!.GraphicsQueue,
							1,
							submitInfo,
							_inFlight
						)) != Result.Success
					) {
						throw new PlatformException($"Could not submit graphics queue: {result}");
					}
				}
			}
			
			// presentation
			var targetSwapchain = vkRenderTarget.Swapchain;
			
			var presentInfo = new PresentInfoKHR {
				WaitSemaphoreCount = 1,
				SwapchainCount = 1,
				PSwapchains = &targetSwapchain,
				PResults = null
			};
			
			fixed(Semaphore* renderCompletePtr = &_renderComplete) {
				presentInfo.PWaitSemaphores = renderCompletePtr;
			}

			fixed(uint* imageIndexPtr = &_imageIndex) {
				presentInfo.PImageIndices = imageIndexPtr;
			}

			VkExtension.Get<KhrSwapchain>(_platform, _platform.PrimaryDevice)
					   .QueuePresent(_platform.PrimaryDevice.SurfaceQueue.Value, &presentInfo);
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);

			unsafe {
				var device = _platform.PrimaryDevice!.Logical;
				
				_platform.API.DestroyRenderPass(device, Base, null);
				_platform.API.DestroyCommandPool(device, CommandPool.Value, null);
				_platform.API.DestroySemaphore(device, _imageAvailable, null);
				_platform.API.DestroySemaphore(device, _renderComplete, null);
				_platform.API.DestroyFence(device, _inFlight, null);
			}
		}
	}
}
