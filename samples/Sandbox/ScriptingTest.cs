using System.Numerics;
using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using Escape;
using Escape.Components;
using Escape.Extensions.Assimp;
using Escape.Primitives;
using Escape.Renderer;
using Escape.Renderer.Camera;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Shader.Pipelines;
using Escape.Resources;
using Escape.Scripting;
using Escape.Scripting.Components;
using Escape.Scripting.Resources;
using Escape.UnitTypes;
using NLog;
using Silk.NET.OpenGL;
using static Shared;
using Camera3D = Escape.Components.Camera3D;

public static class ScriptingTest {

	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		SetupPlatform(GetPlatform(args), out var platform, p => new DefaultUnshadedShaderPipeline(p), out var shaderPipeline, out var renderQueue, out var renderPipeline);
		
		CreateWindow(platform, "Scripting Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);

		// create camera entity
		CreateOrbitalCamera(ref world, window, out var camera, out var orbitalCamera);
		
		//var script = ResourceManager.Load<ScriptResource>(platform, "scripts/test.js")!;
		
		var cube = world.Create3DCube(Color.White, Vector3.Zero, 1, 1, 1);
		//cube.Add(new Scripted(script, null));
		cube.Add(new Scripted(new InternalCSharpScript()));
		
		ESCAPE.Run();
	}

	private class InternalCSharpScript : CSharpScript {

		public override void OnInitialize(Entity e) {
			base.OnInitialize(e);
			
			Console.WriteLine("I am " + e.Id);
		}
		
		public override void OnDeinitialize(Entity e) {
			base.OnDeinitialize(e);
			
			Console.WriteLine("I was " + e.Id);
		}

		public override void OnUpdate(TimeSpan delta) {
			Owner.Get<Transform3D>().Translate(0, 0, (float) delta.TotalSeconds);
		}
	}
}
