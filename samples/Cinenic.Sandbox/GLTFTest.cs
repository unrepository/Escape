using System.Numerics;
using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using Cinenic;
using Cinenic.Components;
using Cinenic.Extensions.Assimp;
using Cinenic.Renderer.Camera;
using Cinenic.Resources;
using Cinenic.UnitTypes;
using NLog;

using static Shared;
using Camera3D = Cinenic.Components.Camera3D;

public static class GLTFTest {

	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		SetupVulkan(out var platform, out var shaderPipeline, out var renderQueue, out var renderPipeline);
		
		CreateWindow(platform, "GLTF Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);
		
		// load scene
		var scene = ResourceManager.Load<AssimpSceneResource>(platform, "/test_models/AntiqueCamera.glb")!;
		var sceneRoot = scene.Get().Scene!.Export(ref world, null);
		
		var q = new QueryDescription().WithNone<Empty>();
		world
			.Query(q, e => {
				Console.WriteLine(e.Id);
				Console.WriteLine(e.GetComponentTypes());
			});
		
		// create camera entity
		CreateOrbitalCamera(ref world, window, out var camera, out var orbitalCamera);
		
		CINENIC.Run();
	}
}
