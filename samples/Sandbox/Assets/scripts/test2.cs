using Arch.Core;
using Escape.Scripting;

[CSharpScript("scripts/test2.cs")]
public class Foo : CSharpScript {

	public override void OnInitialize(World w, Entity e) {
		base.OnInitialize(w, e);

		Logger.Info("Accessing a variable from a different script: {Message}", Bar.MESSAGE);
	}
}
