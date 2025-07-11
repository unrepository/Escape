using System.Runtime.InteropServices;
using NLog;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Cinenic.Renderer.Vulkan {

	public unsafe class VkDevice : IDevice {

		public uint Index { get; }
		public string Name { get; }

		public bool Headless { get; }

		public PhysicalDeviceFeatures Features { get; }

		public Device Logical { get; }
		public PhysicalDevice Physical { get; }
		
		public readonly Queue GraphicsQueue;
		public readonly Queue ComputeQueue;
		public readonly Queue? SurfaceQueue;

		internal int GraphicsFamily { get; } = -1;
		internal int ComputeFamily { get; } = -1;
		internal int SurfaceFamily { get; } = -1;

		internal uint[] QueueFamilies {
			get {
				var indices = new List<uint>();
				
				if(GraphicsFamily > 0) indices.Add((uint) GraphicsFamily);
				if(ComputeFamily > 0) indices.Add((uint) ComputeFamily);
				if(SurfaceFamily > 0) indices.Add((uint) SurfaceFamily);

				return indices.ToArray();
			}
		}

		internal readonly SurfaceCapabilitiesKHR? SurfaceCapabilities;
		internal readonly SurfaceFormatKHR[]? SurfaceFormats;
		internal readonly PresentModeKHR[]? PresentModes;
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly VkPlatform _platform;

		public VkDevice(VkPlatform platform, PhysicalDevice nativeDevice, IList<string>? extensions = null) {
			extensions ??= [];
			
			_platform = platform;
			Physical = nativeDevice;

			if(_platform.CurrentOptions.Headless) {
				Headless = true;
			} else {
				extensions.Add(KhrSwapchain.ExtensionName);
			}

			Features = _platform.API.GetPhysicalDeviceFeature(nativeDevice);

			var properties = _platform.API.GetPhysicalDeviceProperties(nativeDevice);
			Index = properties.DeviceID;
			Name = Marshal.PtrToStringAnsi((nint) properties.DeviceName) ?? "Unknown";

		#region Queue families
			// find queue families
			uint queueFamilyCount = 0;
			_platform.API.GetPhysicalDeviceQueueFamilyProperties(nativeDevice, ref queueFamilyCount, null);

			var queueFamilies = new QueueFamilyProperties[queueFamilyCount];
			_platform.API.GetPhysicalDeviceQueueFamilyProperties(
				nativeDevice,
				&queueFamilyCount,
				new Span<QueueFamilyProperties>(queueFamilies)
			);

			KhrSurface? surfaceKhr = null;
			
			// if not headless, also check for presentation support
			if(!_platform.CurrentOptions.Headless) {
				if(!_platform.API.TryGetInstanceExtension(_platform.Vk, out surfaceKhr)) {
					throw new ApplicationException($"{KhrSurface.ExtensionName} not loaded even though we are not running headlessly?");
				}

				// if(_platform.InitialSurface == null) {
				// 	throw new ArgumentNullException(nameof(_platform.InitialSurface), "An initial window must be created before any device");
				// }
			}

			for(int i = 0; i < queueFamilyCount; i++) {
				var queueFamily = queueFamilies[i];
				
				if(queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit) && GraphicsFamily < 0) {
					GraphicsFamily = i;
				}

				if(queueFamily.QueueFlags.HasFlag(QueueFlags.ComputeBit) && ComputeFamily < 0) {
					ComputeFamily = i;
				}

				if(surfaceKhr != null) {
					surfaceKhr.GetPhysicalDeviceSurfaceSupport(Physical, (uint) i, _platform.InitialSurface, out var surfaceSupport);

					if(surfaceSupport && SurfaceFamily < 0) {
						SurfaceFamily = i;
					}
				}
			}
			
			if(SurfaceFamily < 0) {
				_logger.Warn("Device {Handle} does not support surfaces", Name);
				Headless = true;
			}
			
			// check if required families are present
			if(GraphicsFamily < 0 || ComputeFamily < 0) {
				throw new PlatformNotSupportedException("Required queue families are not supported by this device");
			}
		#endregion
			
		#region Check extension support
			uint deviceExtensionCount = 0;
			Result result;

			if(
				(result = _platform.API.EnumerateDeviceExtensionProperties(
					Physical, (byte*) null,
					ref deviceExtensionCount, null
				))
				!= Result.Success
			) {
				throw new ExternalException($"Could not enumerate device extension properties: {result}");
			}

			var deviceExtensions = new ExtensionProperties[deviceExtensionCount];

			fixed(ExtensionProperties* ptr = &deviceExtensions[0]) {
				if(
					(result = _platform.API.EnumerateDeviceExtensionProperties(
						Physical, (byte*) null,
						ref deviceExtensionCount, ptr
					))
					!= Result.Success
				) {
					throw new ExternalException($"Could not enumerate device extension properties: {result}");
				}
			}

			var availableExtensions =
				deviceExtensions.Select(p => Marshal.PtrToStringAnsi((nint) p.ExtensionName) ?? "")
				                .ToArray();
			
			var enabledExtensions = new List<string>();
			
			foreach(var extension in extensions) {
				if(!availableExtensions.Contains(extension)) {
					_logger.Warn("{DeviceName}: Some extensions are unavailable: {MissingExtension}",
						Name, extension);
					continue;
				}
				
				enabledExtensions.Add(extension);
			}

			if(!enabledExtensions.Contains(KhrSwapchain.ExtensionName)) {
				_logger.Warn("{DeviceName}: {ExtensionName} unavailable, switching to headless mode",
					Name, KhrSwapchain.ExtensionName);
				Headless = true;
			}
		#endregion
			
		#region Check swap chain support on device
			if(!Headless) {
				surfaceKhr!.GetPhysicalDeviceSurfaceCapabilities(Physical, _platform.InitialSurface, out var surfaceCapabilities);
				SurfaceCapabilities = surfaceCapabilities;

				uint formatCount = 0;
				surfaceKhr.GetPhysicalDeviceSurfaceFormats(Physical, _platform.InitialSurface, ref formatCount, null);

				SurfaceFormats = new SurfaceFormatKHR[formatCount];
				fixed(SurfaceFormatKHR* ptr = &SurfaceFormats[0]) {
					surfaceKhr.GetPhysicalDeviceSurfaceFormats(Physical, _platform.InitialSurface, ref formatCount, ptr);
				}

				uint presentModeCount = 0;
				surfaceKhr.GetPhysicalDeviceSurfacePresentModes(Physical, _platform.InitialSurface, ref presentModeCount, null);

				PresentModes = new PresentModeKHR[presentModeCount];
				fixed(PresentModeKHR* ptr = &PresentModes[0]) {
					surfaceKhr.GetPhysicalDeviceSurfacePresentModes(Physical, _platform.InitialSurface, ref presentModeCount, ptr);
				}

				if(formatCount == 0 || presentModeCount == 0) {
					_logger.Info("{DeviceName}: No surface formats or present modes available", Name);
					Headless = false;
				}
			}
		#endregion

		#region Logical device creation
			var queueInfos = new List<DeviceQueueCreateInfo>();
			
			// graphics family
			float priority = 1.0f;
			
			queueInfos.Add(new DeviceQueueCreateInfo() {
				SType = StructureType.DeviceQueueCreateInfo,
				QueueFamilyIndex = (uint) GraphicsFamily,
				QueueCount = 1,
				PQueuePriorities = &priority
			});
			
			// compute family
			queueInfos.Add(new DeviceQueueCreateInfo() {
				SType = StructureType.DeviceQueueCreateInfo,
				QueueFamilyIndex = (uint) ComputeFamily,
				QueueCount = 1,
				PQueuePriorities = &priority
			});

			// surface family, if present
			if(SurfaceFamily > 0) {
				queueInfos.Add(new DeviceQueueCreateInfo() {
					SType = StructureType.DeviceQueueCreateInfo,
					QueueFamilyIndex = (uint) SurfaceFamily,
					QueueCount = 1,
					PQueuePriorities = &priority
				});
			}
			
			// device creation
			var deviceFeatures = new PhysicalDeviceFeatures();

			var deviceInfo = new DeviceCreateInfo() {
				SType = StructureType.DeviceCreateInfo,
				QueueCreateInfoCount = (uint) queueInfos.Count,
				PEnabledFeatures = &deviceFeatures,
				EnabledExtensionCount = (uint) enabledExtensions.Count,
				PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(enabledExtensions)
			};

			fixed(DeviceQueueCreateInfo* ptr = &queueInfos.ToArray()[0]) {
				deviceInfo.PQueueCreateInfos = ptr;
			}

			if((result = _platform.API.CreateDevice(Physical, deviceInfo, null, out var logicalDevice)) != Result.Success) {
				throw new ExternalException($"Failed to create a logical device: {result}");
			}

			Logical = logicalDevice;
		#endregion

			// required queues
			_platform.API.GetDeviceQueue(Logical, (uint) GraphicsFamily, 0, out var gq);
			GraphicsQueue = gq;
			_platform.API.GetDeviceQueue(Logical, (uint) ComputeFamily, 0, out var cq);
			ComputeQueue = cq;
			
			// surface is optional
			if(SurfaceFamily > 0) {
				_platform.API.GetDeviceQueue(Logical, (uint) SurfaceFamily, 0, out var pq);
				SurfaceQueue = pq;
			}
		}

		public void Dispose() {
			_platform.API.DestroyDevice(Logical, null);
		}
	}
}
