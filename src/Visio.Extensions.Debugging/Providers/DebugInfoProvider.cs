using Visio.Extensions.ImGui;
using Visio.Renderer;
using Hexa.NET.ImGui;

namespace Visio.Extensions.Debugging.Providers {
	
	public abstract class DebugInfoProvider {
		
		public abstract string Title { get; }
		public virtual ImGuiWindowFlags WindowFlags { get; } = ImGuiWindowFlags.HorizontalScrollbar;
		
		public bool IsOpen = false;
		
		public ImGuiController? Controller { get; internal set; }

		public abstract void Render();
	}
}
