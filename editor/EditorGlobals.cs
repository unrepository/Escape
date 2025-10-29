using Escape.Extensions.UI;
using Escape.Resources;

namespace Escape.Editor {
	
	public static class EditorGlobals {
		
		public static ImGuiController ImGuiController { get; internal set; }
		public static bool SingleWindow { get; set; } = false;
	}
}
