using System.Drawing;
using System.Numerics;
using Cinenic;
using Cinenic.Renderer;
using Cinenic.Renderer.Camera;
using Cinenic.Renderer.Shader.Pipelines;
using Cinenic.Renderer.Vulkan;
using Cinenic.Resources;
using Cinenic.World;
using Cinenic.World.Components;
using Flecs.NET.Core;
using NLog;
using static Shared;
using Camera3D = Cinenic.World.Components.Camera3D;

public static class ResourcesTest {
	
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		SetupVulkan(out var platform, out var shaderPipeline, out var renderQueue, out var renderPipeline);

		// load resources
		var texture1 = ResourceManager.Load<TextureResource>(platform, "Assets/Textures/1.png")!;
		var texture1_1 = ResourceManager.Load<TextureResource>(platform, "Assets/Textures/1.png")!;
		var texture1_2 = ResourceManager.Load<TextureResource>(platform, "Assets/Textures/1.png")!;
		var texture2 = ResourceManager.Load<TextureResource>(platform, "Assets/Textures/2.png")!;
		var texture3 = ResourceManager.Load<TextureResource>(platform, "Assets/Textures/3.png")!;
		var texture4 = ResourceManager.Load<TextureResource>(platform, "Assets/Textures/4.png")!;

		// create models
		var model1 = CubeModel.Clone();
		var model2 = CubeModel.Clone();
		var model3 = CubeModel.Clone();
		var model4 = CubeModel.Clone();
		var model5 = CubeModel.Clone();
		var model6 = CubeModel.Clone();

		model1.Meshes[0].Material.AlbedoTexture = texture1.Get();
		model2.Meshes[0].Material.AlbedoTexture = texture1_1.Get();
		model3.Meshes[0].Material.AlbedoTexture = texture1_2.Get();
		model4.Meshes[0].Material.AlbedoTexture = texture2.Get();
		model5.Meshes[0].Material.AlbedoTexture = texture3.Get();
		model6.Meshes[0].Material.AlbedoTexture = texture4.Get();

		CreateWindow(platform, "Resources Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);
		
		// create entities
		world
			.Entity("1")
			.Set(new Transform3D(new Vector3(0, 0, 0), Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model1));
		
		world
			.Entity("2")
			.Set(new Transform3D(new Vector3(-2, 0, 0), Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model2));
		
		world
			.Entity("3")
			.Set(new Transform3D(new Vector3(2, 0, 0), Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model3));
		
		world
			.Entity("4")
			.Set(new Transform3D(new Vector3(0, 2, 0), Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model4));
		
		world
			.Entity("5")
			.Set(new Transform3D(new Vector3(-2, 2, 0), Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model5));
		
		world
			.Entity("6")
			.Set(new Transform3D(new Vector3(2, 2, 0), Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model6));
		
		// create camera entity
		var camera =
			world
				.Entity("camera")
				.Set(new Transform3D(new Vector3(0, 1, -5), Quaternion.Zero, Vector3.One))
				.Set(new Camera3D(
					new PerspectiveCamera3D(window.Framebuffer) {
						FieldOfView = 60
					}
				));
		
		camera.GetMut<Transform3D>().LookAt(new Vector3(0, 1, 0));
		
		// run loop
		CINENIC.Run();
		
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
