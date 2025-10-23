using Escape.Extensions.ImGui;
using ImGui_ = Hexa.NET.ImGui.ImGui;

namespace Escape.Extensions.Debugging.Providers {
	
	public class DemoProvider : DebugInfoProvider {

		public override string Title => "ImGui Demo";

		public override void Render() {
			ImGui_.ShowDemoWindow(ref IsOpen);
		}
	}
}
