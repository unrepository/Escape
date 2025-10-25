using System.Numerics;
using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using Escape;
using Escape.Components;
using Escape.Extensions.Assimp;
using Escape.Primitives;
using Escape.Renderer.Camera;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Shader.Pipelines;
using Escape.Resources;
using Escape.UnitTypes;
using NLog;
using Silk.NET.OpenGL;
using static Shared;
using Camera3D = Escape.Components.Camera3D;

public static class PrimitivesTest {

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

		var pm1 = new PrimitiveManager(world);
		pm1.Add(new Line3D(new Vector3(0, -2, 1), new Vector3(7, 4, 7)));
		pm1.Add(new Icosphere3D(Vector3.Zero, 2, 3));
		
		ESCAPE.Run();
	}
}
