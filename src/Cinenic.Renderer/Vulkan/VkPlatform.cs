using System.Diagnostics;
using System.Runtime.InteropServices;
using NLog;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace Cinenic.Renderer.Vulkan {
	
	public unsafe class VkPlatform : IPlatform {
		
		public bool IsInitialized { get; set; } = false;
		public Options CurrentOptions { get; }

		public Vk API { get; set; }
		public Instance VK { get; private set; }

		public VkDevice? PrimaryDevice {
			get;
			set {
				Debug.Assert(value != null);
				
				field = value;
				_logger.Info("Primary device is now {DeviceName}", value.Name);
			}
		}
		
		internal SurfaceKHR InitialSurface { get; private set; }
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private readonly List<string> _vkExtensions = [];
		private readonly List<string> _vkLayers = [];

		private DebugUtilsMessengerEXT _debugMessenger;

		public VkPlatform(PlatformOptions? options = null) {
			CurrentOptions = options as Options ?? new Options();
			API = Vk.GetApi();

		#region Layer validation
			uint layerCount = 0;
			Result result;

			if((result = API.EnumerateInstanceLayerProperties(ref layerCount, null)) != Result.Success) {
				throw new ExternalException($"Could not enumerate instance layer properties: {result}", (int) result);
			}

			var layerProperties = new LayerProperties[layerCount];

			if((result = API.EnumerateInstanceLayerProperties(ref layerCount, ref layerProperties[0])) != Result.Success) {
				throw new ExternalException($"Could not enumerate instance layer properties: {result}", (int) result);
			}
			
			foreach(var layer in CurrentOptions.Layers) {
				var exists = layerProperties.Any(
					layerProperty =>
						Marshal.PtrToStringAnsi((nint) layerProperty.LayerName) == layer
				);

				if(exists) {
					_vkLayers.Add(layer);
					continue;
				}
				
				_logger.Warn("Vulkan layer not available: {Layer}", layer);
			}
		#endregion

			if(!CurrentOptions.Headless) {
				// enable required extensions for windowing
				// doing it this way is a bit silly, but if it works it works
				var window = Silk.NET.Windowing.Window.Create(WindowOptions.DefaultVulkan);
				window.Initialize();
				
				var windowExtensions = window.VkSurface!.GetRequiredExtensions(out var windowExtensionCount);

				for(int i = 0; i < windowExtensionCount; i++) {
					var cExtensionName = windowExtensions[i];
					var extensionName = Marshal.PtrToStringAnsi((nint) cExtensionName);

					if(extensionName != null) {
						_vkExtensions.Add(extensionName);
					}
				}
				
				window.Dispose();
			}
			
		#region Extension validation
			// TODO
			//_api.IsInstanceExtensionPresent()
			_vkExtensions.AddRange(CurrentOptions.Extensions);
		#endregion
		}

		public void Initialize() {
			var appInfo = new ApplicationInfo() {
				SType = StructureType.ApplicationInfo,
				PApplicationName = (byte*) Marshal.StringToHGlobalAnsi("CINENIC"),
				ApplicationVersion = new Version32(1, 0, 0),
				PEngineName = (byte*) Marshal.StringToHGlobalAnsi("N/A"),
				EngineVersion = new Version32(1, 0, 0),
				ApiVersion = Vk.Version10,
			};

			var instanceInfo = new InstanceCreateInfo() {
				SType = StructureType.InstanceCreateInfo,
				PApplicationInfo = &appInfo,
				EnabledExtensionCount = (uint) _vkExtensions.Count,
				PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(_vkExtensions),
				EnabledLayerCount = (uint) _vkLayers.Count,
				PpEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(_vkLayers),
			};

			Result result;

			if((result = API.CreateInstance(&instanceInfo, null, out var vk)) != Result.Success) {
				throw new ExternalException($"Failed to initialize Vulkan: {result}", (int) result);
			}

			VK = vk;

			// free the stuff we allocated earlier
			/*Marshal.FreeHGlobal((nint) appInfo.PApplicationName);
			Marshal.FreeHGlobal((int) appInfo.PEngineName);
			SilkMarshal.Free((nint) instanceInfo.PpEnabledExtensionNames);
			SilkMarshal.Free((nint) instanceInfo.PpEnabledLayerNames);*/

			if(_vkExtensions.Contains(ExtDebugUtils.ExtensionName)) {
				// set up message callback
				if(API.TryGetInstanceExtension(VK, out ExtDebugUtils debugUtils)) {
					var messengerInfo = new DebugUtilsMessengerCreateInfoEXT() {
						SType = StructureType.DebugUtilsMessengerCreateInfoExt,
						MessageSeverity =
							DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt
							| DebugUtilsMessageSeverityFlagsEXT.WarningBitExt
							| DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt
							| DebugUtilsMessageSeverityFlagsEXT.InfoBitExt,
						MessageType =
							DebugUtilsMessageTypeFlagsEXT.GeneralBitExt
							| DebugUtilsMessageTypeFlagsEXT.ValidationBitExt
							| DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
						PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT) DebugMessageCallback,
					};

					fixed(DebugUtilsMessengerEXT* debugMessenger = &_debugMessenger) {
						if(
							(result = debugUtils.CreateDebugUtilsMessenger(
								VK,
								&messengerInfo,
								null,
								debugMessenger
							)) != Result.Success
						) {
							_logger.Error("Couldn't create the debug messenger: {Result}", result);
						}
					}
				} else {
					_logger.Error("Failed to load extension {ExtensionName}", ExtDebugUtils.ExtensionName);
				}
			}

			if(!CurrentOptions.Headless) {
				var window = Silk.NET.Windowing.Window.Create(WindowOptions.DefaultVulkan);
				window.Initialize();
				InitialSurface = window.VkSurface!.Create<AllocationCallbacks>(VK.ToHandle(), null).ToSurface();
				window.Close();
			}
			
			IsInitialized = true;
		}

		public IReadOnlyCollection<VkDevice> GetDevices() {
			var nativeDevices = API.GetPhysicalDevices(VK);
			var devices = new List<VkDevice>();

			foreach(var nativeDevice in nativeDevices) {
				devices.Add(new VkDevice(this, nativeDevice));
			}
			
			return devices;
		}

		public VkDevice CreateDevice(int index) {
			var nativeDevices = API.GetPhysicalDevices(VK);

			if(nativeDevices.Count <= 0) {
				throw new PlatformNotSupportedException("No Vulkan-capable devices present");
			}

			if(index >= nativeDevices.Count) {
				_logger.Warn(
					"Selected physical device with index {PhysicalDevice} is out of range, defaulting to 0",
					index
				);

				index = 0;
			}

			return new VkDevice(this, nativeDevices.ElementAt(index));
		}
		
		public void Dispose() {
			GC.SuppressFinalize(this);
			
			if(
				_vkExtensions.Contains(ExtDebugUtils.ExtensionName)
				&& API.TryGetInstanceExtension(VK, out ExtDebugUtils debugUtils)
			) {
				debugUtils.DestroyDebugUtilsMessenger(VK, _debugMessenger, null);
			}
			
			API.DestroyInstance(VK, null);
		}
		
		private uint DebugMessageCallback(
			DebugUtilsMessageSeverityFlagsEXT severityFlags, DebugUtilsMessageTypeFlagsEXT typeFlags,
			DebugUtilsMessengerCallbackDataEXT* callbackData, void* userData
		) {
			_logger.Debug(Marshal.PtrToStringAnsi((nint) callbackData->PMessage));
			return Vk.False;
		}

		public class Options : PlatformOptions {

			public List<string> Extensions { get; set; } = [ ExtDebugUtils.ExtensionName ];
			public List<string> Layers { get; set; } = [];

			public bool Headless { get; set; } = false;
			
			public override void ParseCommandLine(string[] args) {
				throw new NotImplementedException();
			}
		}
	}
}
