using System.Drawing;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Escape;
using Escape.Components;
using Escape.Extensions.Debugging.Providers;
using Escape.Renderer;
using Escape.Renderer.Camera;
using Escape.Renderer.Resources;
using Escape.Renderer.Shader.Pipelines;
using Escape.Renderer.Vulkan;
using Escape.Resources;
using NLog;
using static Shared;
using Camera3D = Escape.Components.Camera3D;

public static class ResourcesTest {
	
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		SetupPlatform(GetPlatform(args), out var platform, out var shaderPipeline, out var renderQueue, out var renderPipeline);

		// load resources
		var texture1 = ResourceManager.Load<TextureResource>(platform, "textures/1.png")!;
		var texture1_1 = ResourceManager.Load<TextureResource>(platform, "textures/1.png")!;
		var texture1_2 = ResourceManager.Load<TextureResource>(platform, "textures/1.png")!;
		var texture2 = ResourceManager.Load<TextureResource>(platform, "textures/2.png")!;
		var texture3 = ResourceManager.Load<TextureResource>(platform, "textures/3.png")!;
		var texture4 = ResourceManager.Load<TextureResource>(platform, "textures/4.png")!;

		// create models
		var model1 = CubeModel.Clone();
		var model2 = CubeModel.Clone();
		var model3 = CubeModel.Clone();
		var model4 = CubeModel.Clone();
		var model5 = CubeModel.Clone();
		var model6 = CubeModel.Clone();

		model1.Meshes[0].Material.AlbedoTexture = texture1;
		model2.Meshes[0].Material.AlbedoTexture = texture1_1;
		model3.Meshes[0].Material.AlbedoTexture = texture1_2;
		model4.Meshes[0].Material.AlbedoTexture = texture2;
		model5.Meshes[0].Material.AlbedoTexture = texture3;
		model6.Meshes[0].Material.AlbedoTexture = texture4;

		CreateWindow(platform, "Resources Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);
		
		// create entities
		world.Create(
			new Transform3D(new Vector3(0, 0, 0), Quaternion.Zero, Vector3.One),
			new RenderableObject(model1)
		);
		
		world.Create(
			new Transform3D(new Vector3(-2, 0, 0), Quaternion.Zero, Vector3.One),
			new RenderableObject(model2)
		);
		
		world.Create(
			new Transform3D(new Vector3(2, 0, 0), Quaternion.Zero, Vector3.One),
			new RenderableObject(model3)
		);
		
		world.Create(
			new Transform3D(new Vector3(0, 2, 0), Quaternion.Zero, Vector3.One),
			new RenderableObject(model4)
		);
		
		world.Create(
			new Transform3D(new Vector3(-2, 2, 0), Quaternion.Zero, Vector3.One),
			new RenderableObject(model5)
		);
		
		world.Create(
			new Transform3D(new Vector3(2, 2, 0), Quaternion.Zero, Vector3.One),
			new RenderableObject(model6)
		);
		
		// create camera entity
		var camera = world.Create(
			new Transform3D(new Vector3(0, 1, -5), Quaternion.Zero, Vector3.One),
			new Camera3D(
				new PerspectiveCamera3D(window) {
					FieldOfView = 60
				}
			)
		);
		
		camera.Get<Transform3D>().LookAt(new Vector3(0, 1, 0));
		
		DebugInterface.Providers.Add(new TransformGizmoProvider(world, camera.Get<Camera3D>()));
		
		// run loop
		ESCAPE.Run();
		
		// test reference counting
		texture1.Dispose();
		texture1_1.Dispose();
		texture3.Dispose();
		texture4.Dispose();
		
		_logger.Info(texture1.IsValid == true);
		_logger.Info(texture1_1.IsValid == true);
		_logger.Info(texture1_2.IsValid == true);
		_logger.Info(texture2.IsValid == true);
		_logger.Info(texture3.IsValid == false);
		_logger.Info(texture4.IsValid == false);
	}
}
