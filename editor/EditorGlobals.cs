using Escape.Extensions.UI;

namespace Escape.Editor {
	
	public static class EditorGlobals {
		
		public static ImGuiController ImGuiController { get; internal set; }
		public static bool SingleWindow { get; set; } = false;
		
		public static DirectoryInfo? ProjectDirectory { get; set; }
	}
}
