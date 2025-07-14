using System.Diagnostics;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkWindow : Window {

		//public static Format DefaultFormat { get; internal set; }
		
		public new VkFramebuffer Framebuffer { get; private set; }
		
		public SurfaceKHR Surface { get; }
		public SwapchainKHR? Swapchain { get; private set; }

		public Format SwapchainFormat { get; private set; }
		public Extent2D SwapchainExtent { get; private set; }
		public Image[] SwapchainImages { get; private set; }
		public ImageView[] SwapchainImageViews { get; private set; }
		public Silk.NET.Vulkan.Framebuffer[] SwapchainFramebuffers { get; private set; }
		
		private readonly VkPlatform _platform;
		private readonly VkDevice _device;
		private readonly KhrSwapchain _khrSwapchain;
		
		public unsafe VkWindow(VkPlatform platform, RenderPipeline pipeline, WindowOptions? options = null)
			: base(platform, pipeline, options)
		{
			Debug.Assert(pipeline is VkRenderPipeline);
			
			_platform = platform;
			Debug.Assert(_platform.PrimaryDevice != null);

			_device = _platform.PrimaryDevice;
			
			if(_device.Headless) {
				throw new PlatformNotSupportedException("Windows cannot be created in a headless environment");
			}

			var windowOptions = options ?? WindowOptions.DefaultVulkan;
			windowOptions.API = GraphicsAPI.DefaultVulkan;

			Base = Silk.NET.Windowing.Window.Create(windowOptions);
			Base.Initialize();
			
			Surface = Base.VkSurface!.Create<AllocationCallbacks>(_platform.Vk.ToHandle(), null).ToSurface();

			if(!_platform.API.TryGetDeviceExtension(_platform.Vk, _device.Logical, out _khrSwapchain)) {
				throw new ExternalException($"Could not load the {KhrSurface.ExtensionName} extension");
			}
			
			_CreateSwapchain();
			_CreateImageViews();
			_CreateFramebuffer();

			//throw new NotImplementedException();
			
		}

		public override void Initialize() {
			throw new NotImplementedException();
		}

		public override double RenderFrame(Action<double>? frameProvider = null) {
			throw new NotImplementedException();
		}

		public override void ScheduleLater(Action action) {
			throw new NotImplementedException();
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			
			if(Swapchain.HasValue) {
				unsafe {
					_khrSwapchain.DestroySwapchain(_device.Logical, Swapchain.Value, null);
				}
			}

			foreach(var imageView in SwapchainImageViews) {
				unsafe {
					_platform.API.DestroyImageView(_device.Logical, imageView, null);
				}
			}

			foreach(var framebuffer in SwapchainFramebuffers) {
				unsafe {
					_platform.API.DestroyFramebuffer(_device.Logical, framebuffer, null);
				}
			}
			
			Base.Close();
			Base.Dispose();
		}

		private unsafe void _CreateSwapchain() {
		#region Pick settings
			SurfaceFormatKHR? format = null;
			PresentModeKHR? mode = null;

			foreach(var surfaceFormat in _device.SurfaceFormats!) {
				if(
					surfaceFormat.Format == ((VkRenderQueue) Pipeline.Queue).VkColorFormat
					&& surfaceFormat.ColorSpace == ((VkRenderQueue) Pipeline.Queue).VkColorSpace
				) {
					format = surfaceFormat;
				}
			}

			foreach(var presentMode in _device.PresentModes!) {
				if(presentMode == PresentModeKHR.FifoKhr) {
					mode = presentMode;
				}
			}

			if(format == null || mode == null) {
				throw new PlatformNotSupportedException("Couldn't pick a valid surface format and mode");
			}

			Extent2D swapExtent;
			var surfaceCapabilities = _device.SurfaceCapabilities!.Value;

			if(surfaceCapabilities.CurrentExtent.Width != uint.MaxValue) {
				swapExtent = surfaceCapabilities.CurrentExtent;
			} else {
				swapExtent = new Extent2D() {
					Width = uint.Clamp(
						(uint) Base.FramebufferSize.X,
						surfaceCapabilities.MinImageExtent.Width,
						surfaceCapabilities.MaxImageExtent.Width),
					Height = uint.Clamp(
						(uint) Base.FramebufferSize.Y,
						surfaceCapabilities.MinImageExtent.Height,
						surfaceCapabilities.MaxImageExtent.Height)
				};
			}
		#endregion
			
			uint imageCount = surfaceCapabilities.MinImageCount + 1;

			if(surfaceCapabilities.MaxImageCount > 0 && imageCount > surfaceCapabilities.MaxImageCount) {
				imageCount = surfaceCapabilities.MaxImageCount;
			}

			var swapchainInfo = new SwapchainCreateInfoKHR {
				SType = StructureType.SwapchainCreateInfoKhr,
				Surface = Surface,
				MinImageCount = imageCount,
				ImageFormat = format.Value.Format,
				ImageColorSpace = format.Value.ColorSpace,
				ImageExtent = swapExtent,
				ImageArrayLayers = 1,
				ImageUsage = ImageUsageFlags.ColorAttachmentBit,
				PreTransform = surfaceCapabilities.CurrentTransform,
				CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
				PresentMode = mode.Value,
				Clipped = Vk.True,
			};

			if(Swapchain.HasValue) {
				swapchainInfo.OldSwapchain = Swapchain.Value;
			}
			
			var queueFamilies = _device.QueueFamilies;

			if(_device.GraphicsFamily != _device.SurfaceFamily) {
				swapchainInfo.ImageSharingMode = SharingMode.Concurrent;
				swapchainInfo.QueueFamilyIndexCount = (uint) queueFamilies.Length;

				fixed(uint* ptr = &queueFamilies[0]) {
					swapchainInfo.PQueueFamilyIndices = ptr;
				}
			} else {
				swapchainInfo.ImageSharingMode = SharingMode.Exclusive;
			}
			
			Result result;

			if(
				(result = _khrSwapchain.CreateSwapchain(_device.Logical, swapchainInfo, null, out var swapchain))
				!= Result.Success
			) {
				throw new ExternalException($"Could not create swapchain for window: {result}");
			}

			Swapchain = swapchain;
			SwapchainFormat = swapchainInfo.ImageFormat;
			SwapchainExtent = swapchainInfo.ImageExtent;

		#region Retrieve images
			if(
				(result = _khrSwapchain.GetSwapchainImages(_device.Logical, Swapchain.Value, ref imageCount, null))
				!= Result.Success
			) {
				throw new ExternalException($"Could not retrieve swapchain images: {result}");
			}

			SwapchainImages = new Image[imageCount];

			fixed(Image* ptr = SwapchainImages) {
				if(
					(result = _khrSwapchain.GetSwapchainImages(_device.Logical, Swapchain.Value, ref imageCount, ptr))
					!= Result.Success
				) {
					throw new ExternalException($"Could not retrieve swapchain images: {result}");
				}
			}

		#endregion
		}

		private unsafe void _CreateImageViews() {
			SwapchainImageViews = new ImageView[SwapchainImages.Length];

			for(int i = 0; i < SwapchainImages.Length; i++) {
				var imageViewCreateInfo = new ImageViewCreateInfo() {
					SType = StructureType.ImageViewCreateInfo,
					Image = SwapchainImages[i],
					ViewType = ImageViewType.Type2D,
					Format = SwapchainFormat,
					Components = new ComponentMapping {
						R = ComponentSwizzle.Identity,
						G = ComponentSwizzle.Identity,
						B = ComponentSwizzle.Identity,
						A = ComponentSwizzle.Identity
					},
					SubresourceRange = new ImageSubresourceRange {
						AspectMask = ImageAspectFlags.ColorBit,
						BaseMipLevel = 0,
						LevelCount = 1,
						BaseArrayLayer = 0,
						LayerCount = 1
					}
				};

				fixed(ImageView* ptr = &SwapchainImageViews[i]) {
					Result result;
					
					if(
						(result = _platform.API.CreateImageView(_device.Logical, imageViewCreateInfo, null, ptr))
						!= Result.Success
					) {
						throw new ExternalException($"Could not retrieve swapchain images: {result}");
					}
				}
			}
		}

		private unsafe void _CreateFramebuffer() {
			SwapchainFramebuffers = new Silk.NET.Vulkan.Framebuffer[SwapchainImageViews.Length];
			
			for(int i = 0; i < SwapchainImageViews.Length; i++) {
				var imageView = SwapchainImageViews[i];
				var attachments = new ImageView[] { imageView };

				fixed(ImageView* attachmentsPtr = &attachments[0]) {
					var framebufferInfo = new FramebufferCreateInfo {
						SType = StructureType.FramebufferCreateInfo,
						RenderPass = ((VkRenderQueue) Pipeline.Queue).Base,
						AttachmentCount = 1,
						PAttachments = attachmentsPtr,
						Width = SwapchainExtent.Width,
						Height = SwapchainExtent.Height,
						Layers = 1
					};

					Result result;

					if(
						(result = _platform.API.CreateFramebuffer(
							_device.Logical,
							framebufferInfo,
							null,
							out var swapchainFramebuffer
						)) != Result.Success
					) {
						throw new PlatformException($"Could not create window framebuffer: {result}");
					}

					SwapchainFramebuffers[i] = swapchainFramebuffer;
				}
			}

			Framebuffer = new VkFramebuffer(_platform, Vector2D<uint>.Zero) {
				Base = SwapchainFramebuffers[0],
				Swapchain = Swapchain.Value,
				SwapchainImages = SwapchainImages,
				SwapchainImageViews = SwapchainImageViews,
				SwapchainExtent = SwapchainExtent,
				SwapchainFormat = SwapchainFormat,
				SwapchainFramebuffers = SwapchainFramebuffers
			};
		}
	}
}
