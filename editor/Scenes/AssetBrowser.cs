using System.Diagnostics;
using Arch.Core;
using Escape.Core;
using Escape.Core.Scripting.Components;
using Escape.Core.Scripting.Resources;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Editor.Scenes {
	
	public class AssetBrowser : Scene {

		public AssetBrowser(IPlatform platform, RenderQueue? renderQueue) : base(platform, "asset_browser", null, renderQueue) {
			Debug.Assert(renderQueue is not null);

			var uiScript = ResourceManager.Load<ScriptResource>(platform, "ui/AssetBrowser.cs")!;
			World.Create(new Scripted(uiScript));
		}
	}
}
