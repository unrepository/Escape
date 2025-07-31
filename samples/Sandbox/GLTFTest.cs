using System.Numerics;
using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using Visio;
using Visio.Components;
using Visio.Extensions.Assimp;
using Visio.Renderer.Camera;
using Visio.Resources;
using Visio.UnitTypes;
using NLog;

using static Shared;
using Camera3D = Visio.Components.Camera3D;

public static class GLTFTest {

	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		SetupVulkan(out var platform, out var shaderPipeline, out var renderQueue, out var renderPipeline);
		
		CreateWindow(platform, "GLTF Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);
		
		// load scene
		var scene = ResourceManager.Load<AssimpSceneResource>(platform, "/test_models/Corset.glb")!;
		var sceneRoot = scene.Get().Scene!.Export(ref world, null);
		
		var q = new QueryDescription().WithNone<Empty>();
		world
			.Query(q, e => {
				Console.WriteLine(e.Id);
				Console.WriteLine(e.GetComponentTypes());
			});
		
		// create camera entity
		CreateOrbitalCamera(ref world, window, out var camera, out var orbitalCamera);
		
		VISIO.Run();
	}
}
