using NLog;
using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkRenderQueue : RenderQueue {
		
		public Silk.NET.Vulkan.RenderPass Base;
			
		public readonly List<AttachmentDescription> Attachments = [];
		public readonly List<SubpassDescription> Subpasses = [];

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly VkPlatform _platform;

		public VkRenderQueue(VkPlatform platform, Family family) : base(platform, family) {
			_platform = platform;
		}
		
		public unsafe override void Initialize() {
			var attachments = Attachments.ToArray();
			var subpasses = Subpasses.ToArray();

			fixed(AttachmentDescription* attachmentsPtr = &attachments[0]) {
				fixed(SubpassDescription* subpassesPtr = &subpasses[0]) {
					var renderPassInfo = new RenderPassCreateInfo {
						SType = StructureType.RenderPassCreateInfo,
						AttachmentCount = (uint) attachments.Length,
						PAttachments = attachmentsPtr,
						SubpassCount = (uint) subpasses.Length,
						PSubpasses = subpassesPtr
					};

					Result result;
					if(
						(result = _platform.API.CreateRenderPass(_platform.PrimaryDevice!.Logical, &renderPassInfo, null, out var pass))
						!= Result.Success
					) {
						throw new PlatformException($"Could not create render pass: {result}");
					}

					Base = pass;
				}
			}
			
			_logger.Debug("Initialized");
		}

		public void CreateAttachment(Format format) {
			var attachment = new AttachmentDescription {
				Format = format,
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

		public unsafe void CreateSubpass(uint attachmentIndex, ImageLayout layout, SubpassDescription description) {
			var attachmentReference = new AttachmentReference {
				Attachment = attachmentIndex,
				Layout = layout
			};

			description.PColorAttachments = &attachmentReference;
			description.PInputAttachments = &attachmentReference;
			
			Subpasses.Add(description);
			
			_logger.Debug("Created subpass for {RenderPass}, attachmentIndex={AttachmentIndex}, layout={Layout}, bindPoint={BindPoint}",
				this, attachmentIndex, layout, description.PipelineBindPoint);
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);

			unsafe {
				_platform.API.DestroyRenderPass(_platform.PrimaryDevice.Logical, Base, null);
			}
		}
	}
}
