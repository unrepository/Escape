using System;
using Arch.Core;
using Arch.Core.Extensions;
using Escape.Core.Components;
using Escape.Core.Scripting;
using Escape.Renderer;

[CSharpScript("scripts/test.cs")]
public class ExternalCSharpScript : CSharpScript {

	public int PropertyTest { get; set; } = 17;
	
	public override void OnInitialize(IPlatform p, World w, Entity e) {
		base.OnInitialize(p, w, e);
		
		Console.WriteLine("External: I am " + e.Id);
	}
	
	public override void OnDeinitialize(IPlatform p, World w, Entity e) {
		base.OnDeinitialize(p, w, e);
			
		Console.WriteLine("External: I was " + e.Id);
	}

	public override void OnUpdate(TimeSpan delta) {
		Owner.Get<Transform3D>().Translate(0, 0, 0.5f * (float) delta.TotalSeconds);
	}
}
