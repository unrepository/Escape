using Arch.Core;
using Arch.Core.Extensions;
using Escape;
using Escape.Components;
using Escape.Extensions.Assimp;
using Escape.Renderer;
using Escape.Renderer.Shader;
using Escape.Renderer.Shader.Pipelines;
using Escape.Resources;
using NLog;
using Silk.NET.Windowing;
using static Shared;

public static class MultiPlatformTest {
	
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		IPlatform platform = null;
		DefaultSceneShaderPipeline shaderPipeline = null;
		RenderQueue renderQueue = null;
		RenderPipeline renderPipeline = null;
		
		if(args[0] == "gl") {
			SetupOpenGL(out var glPlatform, out shaderPipeline, out renderQueue, out renderPipeline);
			platform = glPlatform;
		} else {
			SetupVulkan(out var vkPlatform, out shaderPipeline, out renderQueue, out renderPipeline);
			platform = vkPlatform;
		}
		
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
		
		RenderManager.Add(renderQueue, "window", (_, _) => {
			window.Base.MakeCurrent();
			window.Base.DoEvents();
			window.Base.DoRender();
		});
		
		ESCAPE.Run();
	}
}
