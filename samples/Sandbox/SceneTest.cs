using System.Numerics;
using Arch.Core;
using Escape.Core;
using Escape.Extensions.Primitives;
using Escape.Renderer;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Shader.Pipelines;
using NLog;
using Silk.NET.OpenGL;

using static Shared;

public static class SceneTest {

	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		SetupPlatform(GetPlatform(args), out var platform, p => new DefaultUnshadedShaderPipeline(p), out var shaderPipeline, out var renderQueue, out var renderPipeline);

		if(renderPipeline is GLRenderPipeline glRenderPipeline) {
			glRenderPipeline.StateSetup = (_, glPlatform) => {
				glPlatform.API.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
			};
		}
		
		CreateWindow(platform, "Scene Test", ref renderQueue, out var window);
		
		SceneEngine.SetScene(renderQueue, new TestScene(platform, renderQueue, window));
		ESCAPE.Run();
	}

	private class TestScene : Scene {

		public TestScene(IPlatform platform, RenderQueue renderQueue, Window window) : base(platform, "test", null, renderQueue) {
			World.Create3DCube(Color.White, Vector3.Zero, 1, 1, 1);

			var world = World;
			CreateOrbitalCamera(ref world, window, out var camera, out var orbitalCamera);
		}

		public override void OnOpen() {
			Console.WriteLine("Test scene open");
		}
		
		public override void OnClose() {
			Console.WriteLine("Test scene close");
		}

		public override void Dispose() {
			Console.WriteLine("Test scene dispose");
			
			base.Dispose();
		}
	}
}
