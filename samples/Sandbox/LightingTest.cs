using System.Drawing;
using System.Numerics;
using Arch.Core.Extensions;
using Escape;
using Escape.Components;
using Escape.Extensions.Assimp;
using Escape.Extensions.Scene;
using Escape.Renderer;
using Escape.Renderer.Camera;
using Escape.Renderer.Resources;
using Escape.Resources;
using Escape.UnitTypes;
using static Shared;
using Camera3D = Escape.Components.Camera3D;
using Color = Escape.Renderer.Color;

public static class LightingTest {

	public static void Start(string[] args) {
		SetupVulkan(out var platform, out var shaderPipeline, out var renderQueue, out var renderPipeline);
		
		CreateWindow(platform, "Lighting Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);

		// var scene = ResourceManager.Load<AssimpSceneResource>(platform, "/test_models/pkg_a_curtains/NewSponza_Curtains_glTF.gltf");
		// scene.Get().Scene!.Export(ref world, null);
		
		// test objects
		// var m00 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/rocks.glb")!;
		// var e00 = m00.Get().Scene!.Export(ref world, null);
		// e00.Add(new Transform3D(new Vector3(0, 0, 0), Quaternion.Identity, new Vector3(2)));
		// e00.GetChild(index: 0).Get<RenderableObject>().Model.Meshes[0].Material.DisplacementTexture = ResourceManager.Load<TextureResource>(platform, "/textures/Rocks019_4K-JPG_Displacement.jpg");
		
		var m01 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/brick_cube.glb")!;
		var e01 = m01.Get().Scene!.Export(ref world, null);
		e01.Add(new Transform3D(new Vector3(0, 0.7f, 0.1f), Quaternion.Identity, new Vector3(0.2f)));
		e01.GetChild(index: 0).Get<RenderableObject>().Model.Meshes[0].Material.HeightTexture = ResourceManager.Load<TextureResource>(platform, "/textures/Bricks059_1K-JPG/Bricks059_1K-JPG_Displacement.jpg");
		
		var m02 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/concrete_cube.glb")!;
		var e02 = m02.Get().Scene!.Export(ref world, null);
		e02.Add(new Transform3D(new Vector3(-0.5f, 0.7f, 0.1f), Quaternion.Identity, new Vector3(0.2f)));
		e02.GetChild(index: 0).Get<RenderableObject>().Model.Meshes[0].Material.HeightTexture = ResourceManager.Load<TextureResource>(platform, "/textures/Concrete041B_1K-JPG_Displacement.jpg");
		
		var m03 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/onyx_cube.glb")!;
		var e03 = m03.Get().Scene!.Export(ref world, null);
		e03.Add(new Transform3D(new Vector3(0.5f, 0.7f, 0.1f), Quaternion.Identity, new Vector3(0.2f)));
		
		// var m4 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/metal_sphere.glb")!;
		// var e4 = m4.Get().Scene!.Export(ref world, null);
		// e4.Add(new Transform3D(new Vector3(-2, 2, 0), Quaternion.Identity, new Vector3(0.5f)));
		//
		// var m5 = ResourceManager.Load<AssimpSceneResource>(platform, "/models/wood_cube.glb")!;
		// var e5 = m5.Get().Scene!.Export(ref world, null);
		// e5.Add(new Transform3D(new Vector3(-2, -2, 0), Quaternion.Identity, new Vector3(0.5f)));
		
		var m1 = ResourceManager.Load<AssimpSceneResource>(platform, "/test_models/models/sofa_03_2k.gltf/sofa_03_2k.gltf")!;
		var e1 = m1.Get().Scene!.Export(ref world, null);
		e1.Add(new Transform3D(new Vector3(0, 0, 0), rotation: Quaternion.Identity));
		
		var m2 = ResourceManager.Load<AssimpSceneResource>(platform, "/test_models/models/cardboard_box_01_2k.gltf/cardboard_box_01_2k.gltf")!;
		var e2 = m2.Get().Scene!.Export(ref world, null);
		e2.Add(new Transform3D(new Vector3(-1.2f, 0, 0.85f), yaw: Rotation<float>.FromDegrees(37)));
		
		var m3 = ResourceManager.Load<AssimpSceneResource>(platform, "/test_models/models/Television_01_2k.gltf/Television_01_2k.gltf")!;
		var e3 = m3.Get().Scene!.Export(ref world, null);
		e3.Add(new Transform3D(new Vector3(0.35f, 0.523f, 1.25f), yaw: Rotation<float>.FromDegrees(-72), scale: new Vector3(0.75f)));
		
		var m4 = ResourceManager.Load<AssimpSceneResource>(platform, "/test_models/models/CoffeeTable_01_2k.gltf/CoffeeTable_01_2k.gltf")!;
		var e4 = m4.Get().Scene!.Export(ref world, null);
		e4.Add(new Transform3D(new Vector3(0, 0, 1.23f), yaw: Rotation<float>.FromDegrees(3)));
		
		var m5 = ResourceManager.Load<AssimpSceneResource>(platform, "/test_models/models/brass_goblets_2k.gltf/brass_goblets_2k.gltf")!;
		var e5 = m5.Get().Scene!.Export(ref world, null);
		e5.Add(new Transform3D(new Vector3(-0.3f, 0.523f, 1.23f), yaw: Rotation<float>.FromDegrees(0)));

		var m6 = ResourceManager.Load<AssimpSceneResource>(platform, "/test_models/models/mid_century_lounge_chair_2k.gltf/mid_century_lounge_chair_2k.gltf")!;
		var e6 = m6.Get().Scene!.Export(ref world, null);
		e6.Add(new Transform3D(new Vector3(1.8f, 0, 1.1f), yaw: Rotation<float>.FromDegrees(-60)));
		
		var lightObject = ResourceManager.Load<AssimpSceneResource>(platform, "/models/alpha_sphere.glb")!;
		
		// lights
		var dir = world.Create(
			new Transform3D(),
			new DirectionalLight(Color.White, Intensity: 2)
		);
		dir.Get<Transform3D>().Yaw = Rotation<float>.FromDegrees(-35);
		dir.Get<Transform3D>().Pitch = Rotation<float>.FromDegrees(50);
		
		// directional light "sun" visualisation
		//lightObject.Get().Scene!.Export(ref world, dir);
		dir.Get<Transform3D>().Scale = new Vector3(2);
		dir.Get<Transform3D>().Position = Vector3.Transform(-Vector3.UnitZ, dir.Get<Transform3D>().Rotation) * 5;
		
		var point = world.Create(
			new Transform3D(position: new Vector3(0, 1, 1), rotation: Quaternion.Identity),
			new PointLight(new Color(255, 255, 50), Intensity: 1)
		);
		//lightObject.Get().Scene!.Export(ref world, point);
		
		var spot = world.Create(
			new Transform3D(new Vector3(1.8f, 1.5f, 1.1f), Quaternion.Identity, Vector3.One),
			new SpotLight(new Color(255, 0, 0), intensity: 3, cutoff: Rotation<float>.FromDegrees(45))
		);
		spot.Get<Transform3D>().Pitch = Rotation<float>.FromDegrees(90);
		//lightObject.Get().Scene!.Export(ref world, spot);
		
		CreateOrbitalCamera(ref world, window, out var cameraEntity, out var oc3d);
		oc3d.Target = new Vector3(0, 0.5f, 1);
		oc3d.Distance = 3.2f;
		//cameraEntity.Get<Transform3D>().Rotate(pitch: Rotation<float>.FromDegrees(35), yaw: Rotation<float>.FromDegrees(60));
		
		// move around point light
		UpdateManager.Add("point light move", _ => {
			point.Get<Transform3D>().Position = new Vector3(
				MathF.Sin((float) window.Base.Time) * 2,
				1.2f,
				0.5f + MathF.Cos((float) window.Base.Time) * 2
			);
		});

		/*bool a = false;
		
		UpdateManager.Add("removal test", _ => {
			if(a) return;
			if(window.Base.Time > 15) {
				world.Destroy(e1);
				a = true;
			}
		});*/
		
		ESCAPE.Run();
	}
}
