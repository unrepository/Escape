using Arch.Core;
using Escape.Core.Scripting;

[CSharpScript("scripts/test2.cs")]
public class Foo : CSharpScript {

	public Foo(int number, string text) {
		Logger.Info("Got constructor arguments: {A}, {B}", number, text);
	}
	
	public override void OnInitialize(World w, Entity e) {
		base.OnInitialize(w, e);

		Logger.Info("Accessing a variable from a different script: {Message}", Bar.MESSAGE);
	}
}
