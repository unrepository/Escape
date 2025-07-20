using System.Diagnostics;
using Cinenic.Extensions.CSharp;
using Silk.NET.Maths;
using Silk.NET.Vulkan;

using static Cinenic.Renderer.Vulkan.VkHelpers;

// TODO
namespace Cinenic.Renderer.Vulkan {
	
	public class VkFramebuffer : Framebuffer {
		
		public Silk.NET.Vulkan.Framebuffer Base { get; protected set; }

		protected Dictionary<Image, (ImageView View, DeviceMemory Memory)> AttachmentImages = [];
		
		private readonly VkPlatform _platform;
		private readonly VkRenderQueue _queue;
		private readonly Device _device;
		
		public VkFramebuffer(VkPlatform platform, RenderQueue queue, Vector2D<uint> size) : base(platform, queue, size) {
			_platform = platform;
			_queue = (VkRenderQueue) queue;
			_device = platform.PrimaryDevice.Logical;
		}
		
		public override void Bind() {
			throw new NotSupportedException();
		}
		
		public override void Unbind() {
			throw new NotSupportedException();
		}
		
		public override void CreateAttachment(AttachmentType type) {
			var (image, view, memory) = CreateAttachment(type, null);
			AttachmentImages[image] = (view, memory);
		}

		public unsafe (Image Image, ImageView View, DeviceMemory Memory) CreateAttachment(AttachmentType type, Image? attachmentImage) {
			Debug.Assert(Handle == 0, "Cannot create attachments after framebuffer has been created!");
			
			Format format;
			ImageUsageFlags usageFlags;
			ImageAspectFlags aspectFlags;

			switch(type) {
				case AttachmentType.Color:
					format = _queue.VkColorFormat;
					usageFlags = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.SampledBit;
					aspectFlags = ImageAspectFlags.ColorBit;
					break;
				case AttachmentType.Depth:
					format = Format.D32Sfloat;
					usageFlags = ImageUsageFlags.DepthStencilAttachmentBit;
					aspectFlags = ImageAspectFlags.DepthBit;
					break;
				default:
					throw new NotImplementedException();
			}

			Image image;
			DeviceMemory imageMemory = default;

			if(attachmentImage is null) {
				// create image
				var imageInfo = new ImageCreateInfo {
					SType = StructureType.ImageCreateInfo,
					ImageType = ImageType.Type2D,
					Format = format,
					Extent = {
						Width = Size.X,
						Height = Size.Y,
						Depth = 1
					},
					MipLevels = 1,
					ArrayLayers = 1,
					Samples = SampleCountFlags.Count1Bit,
					Tiling = ImageTiling.Optimal,
					Usage = usageFlags,
					SharingMode = SharingMode.Exclusive,
					InitialLayout = ImageLayout.Undefined
				};

				VkCheck(
					_platform.API.CreateImage(
						_device,
						imageInfo,
						null,
						out image
					),
					"Could not create framebuffer attachment image"
				);
				
				// allocate memory
				_platform.API.GetPhysicalDeviceMemoryProperties(_platform.PrimaryDevice.Physical, out var memoryProperties);
				_platform.API.GetImageMemoryRequirements(_device, image, out var memoryRequirements);
			
				var allocateInfo = new MemoryAllocateInfo {
					SType = StructureType.MemoryAllocateInfo,
					AllocationSize = memoryRequirements.Size,
					MemoryTypeIndex = FindMemoryType(memoryProperties, memoryRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
				};

				VkCheck(
					_platform.API.AllocateMemory(
						_device,
						allocateInfo,
						null,
						out imageMemory
					),
					"Could not allocate memory for framebuffer attachment image"
				);
				
				VkCheck(
					_platform.API.BindImageMemory(
						_device,
						image,
						imageMemory,
						0
					),
					"Could not attach memory to framebuffer attachment image"
				);
			} else {
				image = attachmentImage.Value;
			}
			
			// create image view
			var imageViewInfo = new ImageViewCreateInfo {
				SType = StructureType.ImageViewCreateInfo,
				Image = image,
				ViewType = ImageViewType.Type2D,
				Format = format,
				Components = {
					R = ComponentSwizzle.Identity,
					G = ComponentSwizzle.Identity,
					B = ComponentSwizzle.Identity,
					A = ComponentSwizzle.Identity
				},
				SubresourceRange = {
					AspectMask = aspectFlags,
					BaseMipLevel = 0,
					LevelCount = 1,
					BaseArrayLayer = 0,
					LayerCount = 1
				}
			};

			VkCheck(
				_platform.API.CreateImageView(
					_device,
					imageViewInfo,
					null,
					out var imageView
				),
				"Could not create image view for framebuffer attachment image"
			);

			return (image, imageView, imageMemory);
		} 

		public unsafe override void Create() {
			Debug.Assert(Handle == 0);
			Debug.Assert(AttachmentImages.Count > 0);

			var imageViews = new ImageView[AttachmentImages.Count];

			foreach(var (i, (_, (imageView, _))) in AttachmentImages.Enumerate()) {
				imageViews[i] = imageView;
			}

			fixed(ImageView* imageViewsPtr = imageViews) {
				var framebufferInfo = new FramebufferCreateInfo {
					SType = StructureType.FramebufferCreateInfo,
					RenderPass = _queue.Base,
					AttachmentCount = (uint) AttachmentImages.Count,
					PAttachments = imageViewsPtr,
					Width = Size.X,
					Height = Size.Y,
					Layers = 1
				};

				VkCheck(
					_platform.API.CreateFramebuffer(
						_device,
						framebufferInfo,
						null,
						out var framebuffer
					),
					"Could not create framebuffer"
				);

				Base = framebuffer;
				Handle = framebuffer.Handle;
			}
		}

		public override void AttachTexture(Texture texture) {
			throw new NotSupportedException();
		}

		public override void Resize(Vector2D<int> size) {
			throw new NotImplementedException();
		}

		public override byte[] Read(int attachment = 0, Rectangle<uint>? area = null) {
			throw new NotImplementedException();
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);

			unsafe {
				foreach(var (image, (imageView, imageMemory)) in AttachmentImages) {
					_platform.API.DestroyImageView(_device, imageView, null);
					_platform.API.DestroyImage(_device, image, null);

					if(imageMemory.Handle != 0) {
						_platform.API.FreeMemory(_device, imageMemory, null);
					}
				}

				if(Handle != 0) {
					_platform.API.DestroyFramebuffer(_device, Base, null);
					Handle = 0;
				}
			}
			
			AttachmentImages.Clear();
		}
	}
}
