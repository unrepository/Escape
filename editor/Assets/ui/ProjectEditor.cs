using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Escape.Core;
using Escape.Core.Scripting;
using Escape.Editor.Scenes;
using Escape.Renderer;
using Hexa.NET.ImGui;
using BindingFlags = System.Reflection.BindingFlags;

[CSharpScript("ui/ProjectEditor.cs")]
public class ProjectEditor : CSharpScript {

	public Dictionary<Type, (bool, Scene?)> VisibleWindows = new() {
		[ typeof(Escape.Editor.Scenes.AssetBrowser) ] = (true, null)
	};

	private static readonly Regex _pascalTitleCaseConverter = new Regex("(?<=[a-z])([A-Z])");
	
	public override void OnRender(TimeSpan delta, ObjectRenderer objectRenderer) {
		if(ImGui.BeginMainMenuBar()) {
			if(ImGui.BeginMenu("Windows")) {
				foreach(var (type, (visible, scene)) in VisibleWindows) {
					var title = _pascalTitleCaseConverter.Replace(type.Name, " $1");

					if(ImGui.MenuItem(title, visible)) {
						VisibleWindows[type] = (!visible, scene);

						if(VisibleWindows[type].Item1 && scene is null) {
							var ctor = type.GetConstructor(BindingFlags.Public, [ typeof(IPlatform), typeof(RenderQueue) ]);
							VisibleWindows[type] = (true, (Scene) ctor.Invoke(null));
							
						}
					}
				}
				
				ImGui.EndMenu();
			}
			
			ImGui.EndMainMenuBar();
		}

		foreach(var (type, (visible, scene)) in VisibleWindows) {
			if(visible) {
				scene.
			}
		}
	}
}
