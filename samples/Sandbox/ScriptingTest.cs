using System.Numerics;
using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using Escape.Core;
using Escape.Core.Components;
using Escape.Extensions.Assimp;
using Escape.Extensions.Primitives;
using Escape.Renderer;
using Escape.Renderer.Camera;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Shader.Pipelines;
using Escape.Resources;
using Escape.Core.Scripting;
using Escape.Core.Scripting.Components;
using Escape.Core.Scripting.Resources;
using Escape.UnitTypes;
using NLog;
using Silk.NET.OpenGL;
using static Shared;
using Camera3D = Escape.Core.Components.Camera3D;

public static class ScriptingTest {

	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		SetupPlatform(GetPlatform(args), out var platform, p => new DefaultUnshadedShaderPipeline(p), out var shaderPipeline, out var renderQueue, out var renderPipeline);
		
		CreateWindow(platform, "Scripting Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);

		// create camera entity
		CreateOrbitalCamera(ref world, window, out var camera, out var orbitalCamera);
		
		var script = ResourceManager.Load<ScriptResource>(platform, "scripts/test.cs")!;
		var script1 = ResourceManager.Load<ScriptResource>(platform, "scripts/test1.cs")!;
		
		var cube = world.Create3DCube(Color.White, Vector3.Zero, 1, 1, 1);
		cube.Add(new Scripted(new InternalCSharpScript()));
		
		var cube1 = world.Create3DCube(new Color(255, 0, 0), Vector3.Zero, 0.5f, 0.5f, 0.5f);
		cube1.Add(new Scripted(script));
		
		// constructor with default arguments
		var cube2 = world.Create3DCube(new Color(100, 0, 255), Vector3.Zero, 0.5f, 0.5f, 0.5f);
		cube2.Add(new Scripted(script1, [ typeof(float) ], [ Type.Missing ]));
		
		// multiple entities with a single script
		var cube3 = world.Create3DCube(new Color(20, 200, 120), Vector3.Zero, 0.5f, 0.5f, 0.5f);
		cube3.Add(new Scripted(script1, 1.5f));

		var fooScript = ResourceManager.Load<ScriptResource>(platform, "scripts/test2.cs")!;
		
		// custom constructor and accessing outside fields
		var foo = world.Create();
		foo.Add(new Scripted(fooScript, 123, "meow"));
		
		ESCAPE.Run();
	}

	private class InternalCSharpScript : CSharpScript {

		public override void OnInitialize(World w, Entity e) {
			base.OnInitialize(w, e);
			
			Console.WriteLine("I am " + e.Id);
		}
		
		public override void OnDeinitialize(World w, Entity e) {
			base.OnDeinitialize(w, e);
			
			Console.WriteLine("I was " + e.Id);
		}

		public override void OnUpdate(TimeSpan delta) {
			Owner.Get<Transform3D>().Translate(0, 0, (float) delta.TotalSeconds);
		}
	}
}
