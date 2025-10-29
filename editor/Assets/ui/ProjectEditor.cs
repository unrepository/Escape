using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Escape.Core;
using Escape.Core.Components;
using Escape.Core.Scripting;
using Escape.Editor.Scenes;
using Escape.Renderer;
using Hexa.NET.ImGui;
using BindingFlags = System.Reflection.BindingFlags;

[CSharpScript("ui/ProjectEditor.cs")]
public class ProjectEditor : CSharpScript {

	public override void OnRender(RenderQueue queue, TimeSpan delta) {
		if(ImGui.BeginMainMenuBar()) {
			if(ImGui.BeginMenu("Windows")) {
				foreach(var e in World.GetEntities()) {
					if(e.GetName() == null) continue;

					if(ImGui.MenuItem(e.GetName(), e.IsVisible())) {
						e.SetVisible(!e.IsVisible());
					}
				}
				
				ImGui.EndMenu();
			}
			
			ImGui.EndMainMenuBar();
		}
	}
}
