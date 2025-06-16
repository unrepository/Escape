using Eclair.Renderer.Vulkan;
using NLog;
using Silk.NET.Windowing;

namespace GENESIS.Sandbox {
	
	public static class VulkanSandbox {
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public static void Start(string[] args) {
			var platform = new VulkanPlatform(new VulkanPlatform.Options() {
				Headless = false
			});
			platform.Initialize();

			foreach(var device in platform.GetDevices()) {
				_logger.Info("Detected device: {name}", device.Name);
			}
			
			var primaryDevice = platform.CreateDevice(0);
			platform.PrimaryDevice = primaryDevice;
			_logger.Info("Using primary device 0 ({name})", primaryDevice.Name);

			var window = new VulkanWindow(platform, WindowOptions.DefaultVulkan);
		}
	}
}
