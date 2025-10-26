using Arch.Core;
using Arch.Core.Extensions;
using Escape.Components;
using Escape.Scripting;

public class ExternalCSharpScript : CSharpScript {

	public override void OnInitialize(Entity e) {
		base.OnInitialize(e);
			
		Console.WriteLine("External: I am " + e.Id);
	}
		
	public override void OnDeinitialize(Entity e) {
		base.OnDeinitialize(e);
			
		Console.WriteLine("External: I was " + e.Id);
	}

	public override void OnUpdate(TimeSpan delta) {
		Owner.Get<Transform3D>().Translate(0, 0, 0.5f * (float) delta.TotalSeconds);
	}
}

new ExternalCSharpScript()
