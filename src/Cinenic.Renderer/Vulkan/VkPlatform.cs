using System.Diagnostics;
using System.Runtime.InteropServices;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

using static Cinenic.Renderer.Vulkan.VkHelpers;

namespace Cinenic.Renderer.Vulkan {
	
	public unsafe class VkPlatform : IPlatform {

		public Platform Identifier => Platform.Vulkan;

		public Thread PlatformThread { get; set; }
		public bool IsInitialized { get; set; } = false;
		
		public Options CurrentOptions { get; }

		public Vk API { get; }
		public Instance Vk { get; private set; }

		// TODO make non-nullable
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
			API = Silk.NET.Vulkan.Vk.GetApi();

			if(CurrentOptions.EnableValidationLayers) {
				CurrentOptions.Extensions.Add("VK_EXT_validation_features");
				CurrentOptions.Layers.Add("VK_LAYER_KHRONOS_validation");
			}

		#region Layer validation
			uint layerCount = 0;

			VkCheck(
				API.EnumerateInstanceLayerProperties(ref layerCount, null),
				"Could not enumerate instance layer properties"
			);

			var layerProperties = new LayerProperties[layerCount];

			VkCheck(
				API.EnumerateInstanceLayerProperties(ref layerCount, ref layerProperties[0]),
				"Could not enumerate instance layer properties"
			);
			
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
			PlatformThread = Thread.CurrentThread;
			
			var appInfo = new ApplicationInfo {
				SType = StructureType.ApplicationInfo,
				PApplicationName = (byte*) Marshal.StringToHGlobalAnsi("CINENIC"),
				ApplicationVersion = new Version32(1, 0, 0),
				PEngineName = (byte*) Marshal.StringToHGlobalAnsi("N/A"),
				EngineVersion = new Version32(1, 0, 0),
				ApiVersion = Silk.NET.Vulkan.Vk.Version10,
			};

			var instanceInfo = new InstanceCreateInfo {
				SType = StructureType.InstanceCreateInfo,
				PApplicationInfo = &appInfo,
				EnabledExtensionCount = (uint) _vkExtensions.Count,
				PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(_vkExtensions),
				EnabledLayerCount = (uint) _vkLayers.Count,
				PpEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(_vkLayers),
			};

			if(CurrentOptions.EnableValidationLayers) {
				var features = new ValidationFeaturesEXT {
					SType = StructureType.ValidationFeaturesExt,
					EnabledValidationFeatureCount = 1
				};
			
				var enabled = stackalloc ValidationFeatureEnableEXT[1];
				enabled[0] = ValidationFeatureEnableEXT.SynchronizationValidationExt;
				
				features.PEnabledValidationFeatures = enabled;
				instanceInfo.PNext = &features;
			}

			VkCheck(
				API.CreateInstance(&instanceInfo, null, out var vk),
				"Failed to initialise Vulkan"
			);

			Vk = vk;

			// free the stuff we allocated earlier
			/*Marshal.FreeHGlobal((nint) appInfo.PApplicationName);
			Marshal.FreeHGlobal((int) appInfo.PEngineName);
			SilkMarshal.Free((nint) instanceInfo.PpEnabledExtensionNames);
			SilkMarshal.Free((nint) instanceInfo.PpEnabledLayerNames);*/

			if(_vkExtensions.Contains(ExtDebugUtils.ExtensionName)) {
				// set up message callback
				if(API.TryGetInstanceExtension(Vk, out ExtDebugUtils debugUtils)) {
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
						VkCheck(
							debugUtils.CreateDebugUtilsMessenger(
								Vk,
								&messengerInfo,
								null,
								debugMessenger
							),
							"Could not create the debug messenger",
							fatal: false
						);
					}
				} else {
					_logger.Error("Failed to load extension {ExtensionName}", ExtDebugUtils.ExtensionName);
				}
			}

			if(!CurrentOptions.Headless) {
				var window = Silk.NET.Windowing.Window.Create(WindowOptions.DefaultVulkan);
				window.Initialize();
				InitialSurface = window.VkSurface!.Create<AllocationCallbacks>(Vk.ToHandle(), null).ToSurface();
				window.Close();
			}
			
			IsInitialized = true;
		}

		public IReadOnlyCollection<VkDevice> GetDevices() {
			var nativeDevices = API.GetPhysicalDevices(Vk);
			var devices = new List<VkDevice>();

			if(CurrentOptions.DeviceOverride > -1) {
				devices.Add(new VkDevice(this, nativeDevices.ToArray()[CurrentOptions.DeviceOverride]));
				return devices;
			}
			
			foreach(var nativeDevice in nativeDevices) {
				devices.Add(new VkDevice(this, nativeDevice));
			}
			
			return devices;
		}

		public VkDevice CreateDevice(int index) {
			var nativeDevices = API.GetPhysicalDevices(Vk);

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

			var device = new VkDevice(this, nativeDevices.ElementAt(index));
			device.Initialize();
			return device;
		}
		
		public void Dispose() {
			GC.SuppressFinalize(this);
			
			if(
				_vkExtensions.Contains(ExtDebugUtils.ExtensionName)
				&& API.TryGetInstanceExtension(Vk, out ExtDebugUtils debugUtils)
			) {
				debugUtils.DestroyDebugUtilsMessenger(Vk, _debugMessenger, null);
			}
			
			API.DestroyInstance(Vk, null);
		}
		
		private static uint DebugMessageCallback(
			DebugUtilsMessageSeverityFlagsEXT severityFlags, DebugUtilsMessageTypeFlagsEXT typeFlags,
			DebugUtilsMessengerCallbackDataEXT* callbackData, void* userData
		) {
			var message = Marshal.PtrToStringAnsi((nint) callbackData->PMessage)!;

			if(message.Contains("vkCmdBindDescriptorSets") && message.Contains("VkDescriptorSet") && message.Contains("textures")) {
				// we ignore this because yea
				return 0;
			}
			
			_logger.Debug(message);
			return 0;
		}

		public class Options : PlatformOptions {

			public List<string> Extensions { get; set; } = [ ExtDebugUtils.ExtensionName ];
			public List<string> Layers { get; set; } = [];

			public bool Headless { get; set; } = false;
			public bool EnableValidationLayers { get; set; } = true;
			public int DeviceOverride { get; set; } = -1;
			
			public override void ParseCommandLine(string[] args) {
				throw new NotImplementedException();
			}
		}
	}
}
