using System.Drawing;
using System.Numerics;
using Arch.Core.Extensions;
using Cinenic;
using Cinenic.Components;
using Cinenic.Extensions.Assimp;
using Cinenic.Extensions.Scene;
using Cinenic.Renderer;
using Cinenic.Renderer.Camera;
using Cinenic.Renderer.Resources;
using Cinenic.Resources;
using Cinenic.UnitTypes;
using static Shared;
using Camera3D = Cinenic.Components.Camera3D;
using Color = Cinenic.Renderer.Color;

public static class LightingTest {

	public static void Start(string[] args) {
		SetupVulkan(out var platform, out var shaderPipeline, out var renderQueue, out var renderPipeline);
		
		CreateWindow(platform, "Lighting Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);

		// test objects
		var m1 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/brick_cube.glb")!;
		var e1 = m1.Get().Scene!.Export(ref world, null);
		e1.Add(new Transform3D(new Vector3(-2, 0, -2), Quaternion.Identity, new Vector3(0.5f)));
		e1.GetChild(index: 0).Get<RenderableObject>().Model.Meshes[0].Material.DisplacementTexture = ResourceManager.Load<TextureResource>(platform, "/textures/Bricks097_1K-JPG_Displacement.jpg");
		
		var m2 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/concrete_cube.glb")!;
		var e2 = m2.Get().Scene!.Export(ref world, null);
		e2.Add(new Transform3D(new Vector3(-2, 0, 0), Quaternion.Identity, new Vector3(0.5f)));
		e2.GetChild(index: 0).Get<RenderableObject>().Model.Meshes[0].Material.DisplacementTexture = ResourceManager.Load<TextureResource>(platform, "/textures/Concrete041B_1K-JPG_Displacement.jpg");
		
		var m3 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/onyx_cube.glb")!;
		var e3 = m3.Get().Scene!.Export(ref world, null);
		e3.Add(new Transform3D(new Vector3(-2, 0, 2), Quaternion.Identity, new Vector3(1)));
		
		var m4 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/metal_sphere.glb")!;
		var e4 = m4.Get().Scene!.Export(ref world, null);
		e4.Add(new Transform3D(new Vector3(-2, 2, 0), Quaternion.Identity, new Vector3(0.5f)));
		
		var m5 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/wood_cube.glb")!;
		var e5 = m5.Get().Scene!.Export(ref world, null);
		e5.Add(new Transform3D(new Vector3(-2, -2, 0), Quaternion.Identity, new Vector3(0.5f)));
		
		// lights
		var dir = world.Create(
			new Transform3D(),
			new DirectionalLight(Color.White)
		);
		dir.Get<Transform3D>().Yaw = Rotation<float>.FromDegrees(-45);
		dir.Get<Transform3D>().Pitch = Rotation<float>.FromDegrees(20);
		
		var spot = world.Create(
			new Transform3D(new Vector3(-2, 2, 2), Quaternion.Identity, Vector3.One),
			new SpotLight(new Color(255, 255, 100), intensity: 100, cutoff: Rotation<float>.FromDegrees(15))
		);
		spot.Get<Transform3D>().Yaw = Rotation<float>.FromDegrees(-90);
		spot.Get<Transform3D>().Pitch = Rotation<float>.FromDegrees(-45);
		
		world.Create(
			new Transform3D(new Vector3(-5, 0, 0), Quaternion.Identity, Vector3.One),
			new PointLight(new Color(100, 50, 255))
		);
		
		CreateOrbitalCamera(ref world, window, out var cameraEntity, out var oc3d);
		cameraEntity.Get<Transform3D>().LookAt(new Vector3(-2, 0, 0));
		
		CINENIC.Run();
	}
}
