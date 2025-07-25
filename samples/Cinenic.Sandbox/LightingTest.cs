using System.Drawing;
using System.Numerics;
using Arch.Core.Extensions;
using Cinenic;
using Cinenic.Components;
using Cinenic.Extensions.Assimp;
using Cinenic.Extensions.Scene;
using Cinenic.Renderer;
using Cinenic.Renderer.Camera;
using Cinenic.Resources;
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
		
		var m2 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/concrete_cube.glb")!;
		var e2 = m2.Get().Scene!.Export(ref world, null);
		e2.Add(new Transform3D(new Vector3(-2, 0, 0), Quaternion.Identity, new Vector3(0.5f)));
		
		var m3 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/onyx_cube.glb")!;
		var e3 = m3.Get().Scene!.Export(ref world, null);
		e3.Add(new Transform3D(new Vector3(-2, 0, 2), Quaternion.Identity, new Vector3(0.5f)));
		
		var m4 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/metal_sphere.glb")!;
		var e4 = m4.Get().Scene!.Export(ref world, null);
		e4.Add(new Transform3D(new Vector3(-2, 2, 0), Quaternion.Identity, new Vector3(0.5f)));
		
		var m5 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/wood_cube.glb")!;
		var e5 = m5.Get().Scene!.Export(ref world, null);
		e5.Add(new Transform3D(new Vector3(-2, -2, 0), Quaternion.Identity, new Vector3(0.5f)));
		
		// lights
		world.Create(
			new Transform3D(),
			new PointLight(Color.White)
		);
		
		world.Create(
			new Transform3D(new Vector3(-3, 0, -3), Quaternion.Identity, Vector3.One),
			new PointLight(Color.White)
		);
		
		CreateOrbitalCamera(ref world, window, out var cameraEntity, out var oc3d);
		cameraEntity.Get<Transform3D>().LookAt(new Vector3(-2, 0, 0));
		
		CINENIC.Run();
	}
}
