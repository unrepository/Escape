using System;
using Arch.Core;
using Arch.Core.Extensions;
using Escape.Core.Components;
using Escape.Core.Scripting;
using Escape.Renderer;

[CSharpScript("scripts/test1.cs")]
public class SecondCSharpScript : CSharpScript {

	public float SpeedFactor { get; }
	
	public SecondCSharpScript(float speedFactor = 1.0f) {
		SpeedFactor = speedFactor;
		Logger.Info("Speed factor: {SpeedFactor}", SpeedFactor);
	}
	
	public override void OnInitialize(IPlatform p, World w, Entity e) {
		base.OnInitialize(p, w, e);
			
		Logger.Info("External: I am " + e.Id);
	}
		
	public override void OnDeinitialize(IPlatform p, World w, Entity e) {
		base.OnDeinitialize(p, w, e);
			
		Logger.Info("External: I was " + e.Id);
	}

	public override void OnUpdate(TimeSpan delta) {
		Owner.Get<Transform3D>().Translate(0, 0, 0.5f * (float) delta.TotalSeconds * SpeedFactor);
	}
}
