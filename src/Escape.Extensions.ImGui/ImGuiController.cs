using System.Diagnostics;
using System.Runtime.InteropServices;
using Escape.Renderer;
using Escape.Renderer.Vulkan;
using Hexa.NET.ImGui;
using Escape;
using HImGui = Hexa.NET.ImGui.ImGui;

namespace Escape.Extensions.ImGui {
	
	public abstract class ImGuiController : IDisposable {
		
		public string Id { get; }
		public IPlatform Platform { get; }
		public RenderQueue Queue { get; protected set; }
		
		public ImGuiContextPtr Context { get; protected set; }

		public ImGuiIOPtr IO {
			get {
				HImGui.SetCurrentContext(Context);
				return HImGui.GetIO();
			}
		}
		
		public ImGuiPlatformIOPtr PlatformIO {
			get {
				HImGui.SetCurrentContext(Context);
				return HImGui.GetPlatformIO();
			}
		}

		private static readonly Dictionary<RenderQueue, ImGuiController> _controllers = [];
		
		public unsafe ImGuiController(string id, IPlatform platform, RenderQueue queue) {
			Id = id;
			Platform = platform;
			Queue = queue;

			if(_controllers.ContainsKey(queue)) {
				throw new InvalidOperationException("A render queue may have at most 1 ImGui controller!");
			}

			Context = HImGui.CreateContext();
			HImGui.SetCurrentContext(Context);
			
			IO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
			IO.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
			IO.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

			var i = HImGui.GetIO();
			i.IniFilename = (byte*) Marshal.StringToHGlobalAuto($"imgui_{id}.ini");

			HImGui.StyleColorsDark();

			// save settings at exit
			AppDomain.CurrentDomain.ProcessExit += (_, _) => {
				HImGui.SetCurrentContext(Context);
				HImGui.SaveIniSettingsToDisk($"imgui_{id}.ini");

				Dispose();
			};
			
			RenderManager.Add(queue, $"imgui_{id}_begin", (_, _) => {
				Begin();
			}, priority: -1); // begin before everything so we can render at the default priority
			
			RenderManager.Add(queue, $"imgui_{id}_end", (_, _) => {
				End();
			}, priority: 2000); // important to end after object renderer
		}

		public abstract void Begin();
		public abstract void End();

		public abstract void Dispose();

		public static ImGuiController Create(IPlatform platform, string id, RenderQueue queue, Window window) {
			try {
				return platform switch {
					VkPlatform vkPlatform => new VkImGuiController(id, vkPlatform, queue, window),
					_ => throw new NotImplementedException("PlatformImpl")
				};
			} catch(InvalidOperationException) { // controller already exists
				var controller = Get(queue);
				Debug.Assert(controller is not null, "This should never happen!");

				return controller;
			}
		}
		
		public static ImGuiController? Get(RenderQueue queue) {
			if(_controllers.TryGetValue(queue, out var controller)) {
				return controller;
			}

			return null;
		}
	}
}
