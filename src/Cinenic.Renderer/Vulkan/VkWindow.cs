using System.Diagnostics;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

using static Cinenic.Renderer.Vulkan.VkHelpers;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkWindow : Window {

		//public static Format DefaultFormat { get; internal set; }
		
		public SurfaceKHR Surface { get; private set; }

		private readonly VkPlatform _platform;
		private readonly VkDevice _device;
		
		public unsafe VkWindow(VkPlatform platform, WindowOptions? options = null)
			: base(platform, options)
		{
			_platform = platform;
			Debug.Assert(_platform.PrimaryDevice != null);

			_device = _platform.PrimaryDevice;
			
			if(_device.Headless) {
				throw new PlatformNotSupportedException("Windows cannot be created in a headless environment");
			}

			var windowOptions = options ?? WindowOptions.DefaultVulkan;
			windowOptions.API = GraphicsAPI.DefaultVulkan;

			Base = Silk.NET.Windowing.Window.Create(windowOptions);
		}

		public override void Initialize(RenderQueue queue) {
			Debug.Assert(queue is VkRenderQueue);
			var vkQueue = (VkRenderQueue) queue;
			
			Base.FramebufferResize += newSize => {
				VkRenderPipeline.RecreateFramebuffer(
					_platform,
					(VkFramebuffer) queue.RenderTarget,
					out var newFramebuffer
				);

				queue.RenderTarget = newFramebuffer;
				//Base.DoEvents();
			};
			Base.Initialize();

			unsafe {
				Surface = Base.VkSurface!.Create<AllocationCallbacks>(_platform.Vk.ToHandle(), null).ToSurface();
			}
			
			Framebuffer = new WindowFramebuffer(_platform, vkQueue, this, new Vector2D<uint>(Width, Height));
		}

		public override double RenderFrame(Action<double>? frameProvider = null) {
			throw new NotImplementedException();
		}

		public override void ScheduleLater(Action action) {
			throw new NotImplementedException();
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			
			Framebuffer.Dispose();
			
			Base.Close();
			Base.Dispose();
		}

		internal class WindowFramebuffer : VkFramebuffer {
			
			public VkWindow Window { get; }

			private readonly VkPlatform _platform;
			private readonly VkDevice _device;
			
			public WindowFramebuffer(VkPlatform platform, VkRenderQueue queue, VkWindow window, Vector2D<uint> size)
				: base(platform, queue, size)
			{
				_platform = platform;
				Window = window;
				_device = platform.PrimaryDevice!;
				
				_CreateSwapchain();
				_CreateImageViews();
				_CreateFramebuffers();
			}

			private unsafe void _CreateSwapchain() {
			#region Pick settings
				SurfaceFormatKHR? format = null;
				PresentModeKHR? mode = null;

				foreach(var surfaceFormat in _device.SurfaceFormats!) {
					if(
						surfaceFormat.Format == ((VkRenderQueue) Queue).VkColorFormat
						&& surfaceFormat.ColorSpace == ((VkRenderQueue) Queue).VkColorSpace
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
					swapExtent = new Extent2D {
						Width = uint.Clamp(
							Window.Width,
							surfaceCapabilities.MinImageExtent.Width,
							surfaceCapabilities.MaxImageExtent.Width),
						Height = uint.Clamp(
							Window.Height,
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
					Surface = Window.Surface,
					MinImageCount = imageCount,
					ImageFormat = format.Value.Format,
					ImageColorSpace = format.Value.ColorSpace,
					ImageExtent = swapExtent,
					ImageArrayLayers = 1,
					ImageUsage = ImageUsageFlags.ColorAttachmentBit,
					PreTransform = surfaceCapabilities.CurrentTransform,
					CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
					PresentMode = mode.Value,
					Clipped = true,
					OldSwapchain = Swapchain.Handle != 0 ? Swapchain : default
				};
				
				var queueFamilies = _device.QueueFamilies;

				fixed(uint* ptr = queueFamilies) {
					if(_device.GraphicsFamily != _device.SurfaceFamily) {
						swapchainInfo.ImageSharingMode = SharingMode.Concurrent;
						swapchainInfo.QueueFamilyIndexCount = (uint) queueFamilies.Length;
						swapchainInfo.PQueueFamilyIndices = ptr;
					} else {
						swapchainInfo.ImageSharingMode = SharingMode.Exclusive;
					}

					VkCheck(
						VkExtension
							.Get<KhrSwapchain>(_platform, _device)
							.CreateSwapchain(_device.Logical, swapchainInfo, null, out var swapchain),
						"Could not create swapchain"
					);
					
					Swapchain = swapchain;
				}
				
				SwapchainFormat = swapchainInfo.ImageFormat;
				SwapchainExtent = swapchainInfo.ImageExtent;

			#region Retrieve images
				VkCheck(
					VkExtension
						.Get<KhrSwapchain>(_platform, _device)
						.GetSwapchainImages(_device.Logical, Swapchain, ref imageCount, null),
					"Could not retrieve swapchain images"
				);

				SwapchainImages = new Image[imageCount];

				fixed(Image* ptr = SwapchainImages) {
					VkCheck(
						VkExtension
							.Get<KhrSwapchain>(_platform, _device)
							.GetSwapchainImages(_device.Logical, Swapchain, ref imageCount, ptr),
						"Could not retrieve swapchain images"
					);
				}
			#endregion
			}

			private unsafe void _CreateImageViews() {
				SwapchainImageViews = new ImageView[SwapchainImages.Length];

				for(int i = 0; i < SwapchainImages.Length; i++) {
					var imageViewCreateInfo = new ImageViewCreateInfo {
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

					VkCheck(
						_platform.API.CreateImageView(_device.Logical, imageViewCreateInfo, null, out var imageView),
						"Could not create image view"
					);

					SwapchainImageViews[i] = imageView;
				}
			}

			private unsafe void _CreateFramebuffers() {
				SwapchainFramebuffers = new Silk.NET.Vulkan.Framebuffer[SwapchainImageViews.Length];
				
				for(int i = 0; i < SwapchainImageViews.Length; i++) {
					var imageView = SwapchainImageViews[i];
					var attachments = new ImageView[] { imageView };

					fixed(ImageView* attachmentsPtr = &attachments[0]) {
						var framebufferInfo = new FramebufferCreateInfo {
							SType = StructureType.FramebufferCreateInfo,
							RenderPass = ((VkRenderQueue) Queue).Base,
							AttachmentCount = (uint) ((VkRenderQueue) Queue).Attachments.Count,
							PAttachments = attachmentsPtr,
							Width = SwapchainExtent.Width,
							Height = SwapchainExtent.Height,
							Layers = 1
						};

						VkCheck(
							_platform.API.CreateFramebuffer(
								_device.Logical,
								framebufferInfo,
								null,
								out var swapchainFramebuffer
							),
							"Could not create swapchain framebuffer"
						);

						SwapchainFramebuffers[i] = swapchainFramebuffer;
					}
				}

				// Framebuffer = new VkFramebuffer(_platform, Vector2D<uint>.Zero) {
				// 	Base = SwapchainFramebuffers[0],
				// 	Swapchain = Swapchain.Value,
				// 	SwapchainImages = SwapchainImages,
				// 	SwapchainImageViews = SwapchainImageViews,
				// 	SwapchainExtent = SwapchainExtent,
				// 	SwapchainFormat = SwapchainFormat,
				// 	SwapchainFramebuffers = SwapchainFramebuffers
				// };
			}

			public override void Dispose() {
				GC.SuppressFinalize(this);

				unsafe {
					if(Swapchain.Handle != 0) {
						VkExtension
							.Get<KhrSwapchain>(_platform, _device)
							.DestroySwapchain(_device.Logical, Swapchain, null);
					}

					foreach(var image in SwapchainImages) {
						_platform.API.DestroyImage(_device.Logical, image, null);
					}

					foreach(var imageView in SwapchainImageViews) {
						_platform.API.DestroyImageView(_device.Logical, imageView, null);
					}

					foreach(var framebuffer in SwapchainFramebuffers) {
						_platform.API.DestroyFramebuffer(_device.Logical, framebuffer, null);
					}
				}
			}
		}
	}
}
