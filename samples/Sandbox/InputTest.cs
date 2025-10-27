using System.Numerics;
using Escape;
using Escape.Input;
using Escape.Primitives;
using Escape.Renderer;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Shader.Pipelines;
using NLog;
using Silk.NET.Input;
using Silk.NET.OpenGL;

using static Shared;

public static class InputTest {

	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		SetupPlatform(GetPlatform(args), out var platform, p => new DefaultUnshadedShaderPipeline(p), out var shaderPipeline, out var renderQueue, out var renderPipeline);

		if(renderPipeline is GLRenderPipeline glRenderPipeline) {
			glRenderPipeline.StateSetup = (_, glPlatform) => {
				glPlatform.API.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
			};
		}
		
		CreateWindow(platform, "Primitives Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);
		
		// create camera entity
		CreateOrbitalCamera(ref world, window, out var camera, out var orbitalCamera);

		world.Create3DCube(Color.White, Vector3.Zero, 1, 1, 1);

		var aAction = new InputAction("a");
		var bAction = new InputAction("b");
		var cAction = new InputAction("c");
		var dAction = new InputAction("d");
		var eAction = new InputAction("e");

		aAction.Down += action => {
			Console.WriteLine(action.Name + " is down");
		};
		
		bAction.Released += action => {
			Console.WriteLine(action.Name + " was released");
		};
		
		cAction.Released += action => {
			Console.WriteLine(action.Name + " was released");
		};
		
		dAction.Pressed += action => {
			Console.WriteLine(action.Name + " was pressed");
		};
		
		eAction.Pressed += action => {
			Console.WriteLine(action.Name + " was pressed");
		};
		
		UpdateManager.Add("input test", _ => {
			if(bAction.WasPressed) {
				Console.WriteLine("B was pressed");
			}

			if(eAction.WasReleased) {
				Console.WriteLine("E was released");
			}
		});
		
		var inputActions = new Dictionary<InputCombo[], InputAction> {
			[[ new(Key.W) ]] = aAction,
			[[ new(Key.ControlLeft, Key.ShiftLeft, Key.B) ]] = bAction,
			[[ new(Key.ControlLeft, Key.B) ]] = cAction,
			//[[ new(MouseButton.Left) ]] = dAction,
			[[ new(Key.W, Key.E) ]] = eAction,
		};

		var inputMap = new InputMap(
			"default",
			window,
			inputActions
		);
		
		ESCAPE.Run();
	}
}
