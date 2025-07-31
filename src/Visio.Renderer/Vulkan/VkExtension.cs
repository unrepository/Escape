using NLog;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Visio.Renderer.Vulkan {
	
	public static class VkExtension {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private static readonly Dictionary<VkDevice, Dictionary<Type, object>> _deviceExtensionCache = [];
		
		public static T? TryGet<T>(VkPlatform platform, VkDevice device)
			where T : NativeExtension<Vk>
		{
			if(_deviceExtensionCache.TryGetValue(device, out var extensions)) {
				if(extensions.TryGetValue(typeof(T), out var extensionObj)) {
					return (T) extensionObj;
				}
			} else {
				extensions = new Dictionary<Type, object>();
				_deviceExtensionCache[device] = extensions;
			}

			if(!platform.API.TryGetDeviceExtension(platform.Vk, device.Logical, out T extension)) {
				_logger.Warn("Device {Device} does not seem to support {ExtensionType}", device.Name, typeof(T).Name);
			}

			extensions[typeof(T)] = extension;
			return extension;
		}
		
		public static T Get<T>(VkPlatform platform, VkDevice device)
			where T : NativeExtension<Vk>
		{
			var extension = TryGet<T>(platform, device);
			
			if(extension is null) {
				throw new PlatformException($"Device {device.Name} does not support required extension {typeof(T).Name}");
			}

			return extension;
		}
	}
}
