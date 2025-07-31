using System.Diagnostics;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Buffer = Silk.NET.Vulkan.Buffer;
using Image = Silk.NET.Vulkan.Image;

using static Visio.Renderer.Vulkan.VkHelpers;

namespace Visio.Renderer.Vulkan {
	
	public unsafe class VkTexture : Texture {

		public const uint MAX_TEXTURES = 1024;
		
		internal static int LastIndex { get; private set; } = 0;
		internal int Index { get; }
		
		private readonly VkPlatform _platform;

		private Buffer _buffer;
		private DeviceMemory _bufferMemory;
		private readonly uint _bufferSize;
		private void* _bufferData;

		private Image _image;
		private DeviceMemory _imageMemory;
		
		private ImageView _imageView;
		private Sampler _sampler;
		
		public VkTexture(VkPlatform platform, Vector2D<uint> size, TextureFilter filter, TextureWrapMode wrapMode, TextureFormat format)
			: base(platform, size, filter, wrapMode, format)
		{
			_platform = platform;
			_bufferSize = size.X * size.Y * (uint) format;

			Index = LastIndex + 1;
			LastIndex++;

			if(Index > MAX_TEXTURES) {
				throw new IndexOutOfRangeException($"MAX_TEXTURES limit exceeded! {Index} > {MAX_TEXTURES}");
			}
			
			AllocateBuffer(
				platform,
				_bufferSize,
				ref _buffer,
				ref _bufferMemory,
				usageFlags: BufferUsageFlags.TransferSrcBit
			);

			platform.API.MapMemory(
				platform.PrimaryDevice.Logical,
				_bufferMemory,
				0,
				_bufferSize,
				MemoryMapFlags.None,
				ref _bufferData
			);
		}
		
		public override void LoadImage(Image<Rgba32> image) {
			var data = new Rgba32[image.Width * image.Height];
			
			image.ProcessPixelRows(accessor => {
				for(int y = 0; y < image.Height; y++) {
					fixed(Rgba32* addr = accessor.GetRowSpan(image.Height - y - 1)) {
						for(int x = 0; x < image.Width; x++) {
							data[y * image.Width + x] = addr[x];
						}
					}
				}
			});

			fixed(void* dataPtr = data) {
				System.Buffer.MemoryCopy(dataPtr, _bufferData, _bufferSize, _bufferSize);
			}

			var device = _platform.PrimaryDevice.Logical;
			
			_platform.API.UnmapMemory(device, _bufferMemory);
			_bufferMemory = default;

			var imageInfo = new ImageCreateInfo {
				SType = StructureType.ImageCreateInfo,
				ImageType = ImageType.Type2D,
				Extent = {
					Width = (uint) image.Width,
					Height = (uint) image.Height,
					Depth = 1
				},
				MipLevels = 1,
				ArrayLayers = 1,
				Format = Format switch {
					TextureFormat.RGB8 => Silk.NET.Vulkan.Format.R8G8B8Srgb,
					TextureFormat.RGBA8 => Silk.NET.Vulkan.Format.R8G8B8A8Srgb,
					_ => throw new NotImplementedException()
				},
				Tiling = ImageTiling.Optimal,
				InitialLayout = ImageLayout.Undefined,
				Usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit,
				SharingMode = SharingMode.Exclusive,
				Samples = SampleCountFlags.Count1Bit,
				Flags = ImageCreateFlags.None
			};

			VkCheck(
				_platform.API.CreateImage(
					device,
					imageInfo,
					null,
					out _image
				),
				"Could not create image for texture"
			);

			// allocate memory
			_platform.API.GetPhysicalDeviceMemoryProperties(_platform.PrimaryDevice.Physical, out var memoryProperties);
			_platform.API.GetImageMemoryRequirements(device, _image, out var memoryRequirements);

			var allocateInfo = new MemoryAllocateInfo {
				SType = StructureType.MemoryAllocateInfo,
				AllocationSize = memoryRequirements.Size,
				MemoryTypeIndex = FindMemoryType(memoryProperties, memoryRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
			};

			VkCheck(
				_platform.API.AllocateMemory(device, allocateInfo, null, out _imageMemory),
				"Could not allocate memory for image"
			);

			_platform.API.BindImageMemory(device, _image, _imageMemory, 0);
		}
		
		public override void Bind(RenderQueue queue, uint unit) {
			Debug.Assert(queue.Pipeline is not null);
			
			var vkQueue = (VkRenderQueue) queue;
			var vkProgram = (VkShaderProgram) queue.Pipeline.Program.Get();
			var device = _platform.PrimaryDevice.Logical;
			
			// we only want this part to execute once
			if(Handle == 0) {
				vkQueue.CreateSingleTimeCommandAction(
					_platform.PrimaryDevice.GraphicsQueue,
					(_, commandBuffer) => {
						// transition image layout to transfer destination
						TransitionImageLayout(
							_platform,
							commandBuffer,
							_image,
							ImageLayout.Undefined,
							ImageLayout.TransferDstOptimal
						);

						// copy buffer containing texture to image
						var copyRegion = new BufferImageCopy {
							BufferOffset = 0,
							BufferRowLength = 0,
							BufferImageHeight = 0,
							ImageSubresource = {
								AspectMask = ImageAspectFlags.ColorBit,
								MipLevel = 0,
								BaseArrayLayer = 0,
								LayerCount = 1
							},
							ImageOffset = {
								X = 0,
								Y = 0,
								Z = 0
							},
							ImageExtent = {
								Width = Size.X,
								Height = Size.Y,
								Depth = 1
							}
						};
						
						_platform.API.CmdCopyBufferToImage(
							commandBuffer,
							_buffer,
							_image,
							ImageLayout.TransferDstOptimal,
							1,
							copyRegion
						);
						
						// transition layout back to be read by shaders
						TransitionImageLayout(
							_platform,
							commandBuffer,
							_image,
							ImageLayout.TransferDstOptimal,
							ImageLayout.ShaderReadOnlyOptimal
						);
						
						// free temporary texture buffer
						_platform.API.DestroyBuffer(device, _buffer, null);
						_platform.API.FreeMemory(device, _bufferMemory, null);

						// create image view
						var imageViewInfo = new ImageViewCreateInfo {
							SType = StructureType.ImageViewCreateInfo,
							Image = _image,
							ViewType = ImageViewType.Type2D,
							Format = Format switch {
								TextureFormat.RGB8 => Silk.NET.Vulkan.Format.R8G8B8Srgb,
								TextureFormat.RGBA8 => Silk.NET.Vulkan.Format.R8G8B8A8Srgb,
								_ => throw new NotImplementedException()
							},
							SubresourceRange = {
								AspectMask = ImageAspectFlags.ColorBit,
								BaseMipLevel = 0,
								LevelCount = 1,
								BaseArrayLayer = 0,
								LayerCount = 1
							}
						};

						VkCheck(
							_platform.API.CreateImageView(
								device,
								imageViewInfo,
								null,
								out _imageView
							),
							"Could not create image view for texture"
						);
						
						// create samplers
						var samplerInfo = new SamplerCreateInfo {
							SType = StructureType.SamplerCreateInfo,
							MagFilter = Filter switch {
								TextureFilter.Linear => Silk.NET.Vulkan.Filter.Linear,
								TextureFilter.Nearest => Silk.NET.Vulkan.Filter.Nearest,
								_ => throw new NotSupportedException()
							},
							AddressModeU = WrapMode switch {
								TextureWrapMode.ClampToBorder => SamplerAddressMode.ClampToBorder,
								TextureWrapMode.ClampToEdge => SamplerAddressMode.ClampToEdge,
								TextureWrapMode.Repeat => SamplerAddressMode.Repeat,
								TextureWrapMode.RepeatMirrored => SamplerAddressMode.MirroredRepeat
							},
							AnisotropyEnable = _platform.PrimaryDevice.Features.SamplerAnisotropy, // TODO customizable on/off
							MaxAnisotropy = _platform.PrimaryDevice.Properties.Limits.MaxSamplerAnisotropy, // TODO customizable
							BorderColor = BorderColor.IntOpaqueBlack,
							UnnormalizedCoordinates = false,
							CompareEnable = false,
							CompareOp = CompareOp.Always,
							MipmapMode = Filter switch {
								TextureFilter.Linear => SamplerMipmapMode.Linear,
								TextureFilter.Nearest => SamplerMipmapMode.Nearest,
								_ => throw new NotSupportedException()
							},
							MipLodBias = 0.0f,
							MinLod = 0.0f,
							MaxLod = 0.0f,
						};

						samplerInfo.MinFilter = samplerInfo.MagFilter;
						samplerInfo.AddressModeV = samplerInfo.AddressModeW = samplerInfo.AddressModeU;

						VkCheck(
							_platform.API.CreateSampler(
								device,
								samplerInfo,
								null,
								out _sampler
							),
							"Could not create texture sampler"
						);
						
						// update textures descriptor
						var descriptorImageInfo = new DescriptorImageInfo {
							ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
							ImageView = _imageView,
							Sampler = _sampler
						};

						var writeDescriptorSet = new WriteDescriptorSet {
							SType = StructureType.WriteDescriptorSet,
							DstSet = queue.Pipeline.ShaderPipeline.VkTexturesDescriptor,
							DstBinding = unit,
							DstArrayElement = (uint) Index,
							DescriptorType = DescriptorType.CombinedImageSampler,
							DescriptorCount = 1,
							PImageInfo = &descriptorImageInfo
						};
			
						_platform.API.UpdateDescriptorSets(device, 1, writeDescriptorSet, 0, null);
						
						// TODO is there anything to free after uploading to GPU?
					}
				);
				
				Handle = _image.Handle;
				return;
			}
			
			//vkQueue.CreateSingleTimeAction(_ => {
			//	queue.Pipeline.ShaderPipeline.VkBindTextureUnit(unit, _imageView, _sampler);
			//});
		}
		
		public override void Unbind() {
			throw new NotSupportedException();
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);

			var device = _platform.PrimaryDevice.Logical;
			
			_platform.API.DestroySampler(device, _sampler, null);
			_platform.API.DestroyImageView(device, _imageView, null);
			_platform.API.DestroyImage(device, _image, null);
			_platform.API.FreeMemory(device, _imageMemory, null);
		}
	}
}
