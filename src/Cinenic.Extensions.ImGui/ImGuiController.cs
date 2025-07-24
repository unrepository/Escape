using Cinenic.Renderer;
using Hexa.NET.ImGui;

using HImGui = Hexa.NET.ImGui.ImGui;

namespace Cinenic.Extensions.ImGui {
	
	public abstract class ImGuiController : IDisposable {
		
		public IPlatform Platform { get; }
		
		public RenderQueue Queue { get; protected set; }
		public RenderPipeline Pipeline { get; protected set; }
		
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
		
		public ImGuiController(IPlatform platform) {
			Platform = platform;
		}

		// public abstract void Begin();
		// public abstract void End();

		public abstract void Dispose();
	}
}
