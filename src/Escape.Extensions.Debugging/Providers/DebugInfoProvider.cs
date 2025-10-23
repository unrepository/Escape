using Escape.Extensions.ImGui;
using Escape.Renderer;
using Hexa.NET.ImGui;

namespace Escape.Extensions.Debugging.Providers {
	
	public abstract class DebugInfoProvider {
		
		public abstract string Title { get; }
		public virtual ImGuiWindowFlags WindowFlags { get; } = ImGuiWindowFlags.HorizontalScrollbar;
		
		public bool IsOpen = false;
		
		public ImGuiController? Controller { get; internal set; }

		public abstract void Render();
	}
}
