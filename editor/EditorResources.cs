using Escape.Core.Resources;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Editor {
	
	public static class EditorResources {

		public static Ref<ScriptResource> ProjectManagerScript { get; private set; } 
		public static Ref<ScriptResource> ProjectEditorScript { get; private set; }
		public static Ref<ScriptResource> AssetBrowserScript { get; private set; }
		public static Ref<ScriptResource> SceneEditorScript { get; private set; }

		public static void Load(IPlatform platform) {
			ProjectManagerScript = ResourceManager.Load<ScriptResource>(platform, "ui/ProjectManager.cs")!;
			ProjectEditorScript = ResourceManager.Load<ScriptResource>(platform, "ui/ProjectEditor.cs")!;
			AssetBrowserScript = ResourceManager.Load<ScriptResource>(platform, "ui/AssetBrowser.cs")!;
			SceneEditorScript = ResourceManager.Load<ScriptResource>(platform, "ui/SceneEditor.cs")!;
		}
	}
}
