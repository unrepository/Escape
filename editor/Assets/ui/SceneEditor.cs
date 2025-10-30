using System;
using System.IO;
using Escape.Core;
using Escape.Core.Scripting;
using Escape.Renderer;

[CSharpScript("ui/SceneEditor.cs")]
public class SceneEditor : CSharpScript {

	public Scene Scene { get; }

	public SceneEditor(Scene scene) {
		Scene = scene;
	}
	
	public override void OnRender(RenderQueue queue, TimeSpan delta) {
		
	}
}
