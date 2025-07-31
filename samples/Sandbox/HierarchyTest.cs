using System.Drawing;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Visio;
using Visio.Components;
using Visio.Renderer;
using Visio.Renderer.Camera;
using Visio.UnitTypes;
using static Shared;
using Camera3D = Visio.Components.Camera3D;

public static class HierarchyTest {

	public static void Start(string[] args) {
		SetupVulkan(out var platform, out var shaderPipeline, out var renderQueue, out _);
		
		CreateWindow(platform, "Hierarchy Test", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);

		var m1 = CubeModel.Clone();
		m1.Meshes[0].Material.AlbedoColor = System.Drawing.Color.Brown;
		var m2 = CubeModel.Clone();
		m2.Meshes[0].Material.AlbedoColor = System.Drawing.Color.YellowGreen;
		var m3 = CubeModel.Clone();
		m3.Meshes[0].Material.AlbedoColor = System.Drawing.Color.Purple;
		var m4 = CubeModel.Clone();
		m4.Meshes[0].Material.AlbedoColor = System.Drawing.Color.Lavender;
		
		var e1 = world.Create();
		var t3d1 = world.Create(new Transform3D());
		var e2 = world.Create();
		var t3d2 = world.Create(new Transform3D(new Vector3(2, 0, 0), Quaternion.Identity, Vector3.One), new RenderableObject(m1));
		var t3d3 = world.Create(new Transform3D(new Vector3(-2, 0, 0), Quaternion.Identity, Vector3.One), new RenderableObject(m2));
		var t3d4 = world.Create(new Transform3D(new Vector3(0, 0, 0), Quaternion.Identity, Vector3.One), new RenderableObject(m3));
		var t3d5 = world.Create(new Transform3D(new Vector3(0, 0, 2), Quaternion.Identity, Vector3.One), new RenderableObject(m4));
		
		e1.MakeParentOf(t3d1, t3d4);
		
		t3d1.MakeParentOf(e2);
		e2.MakeParentOf(t3d2, t3d3);
		
		t3d4.MakeParentOf(t3d5);
		
		var camera = world.Create(
			new Transform3D(new Vector3(7, 5, 8), Quaternion.Identity, Vector3.One),
			new Camera3D(new PerspectiveCamera3D(window) {
				FieldOfView = 60
			})
		);
		camera.Get<Transform3D>().LookAt(Vector3.Zero);

		var cameraPivot = world.Create(new Transform3D());
		cameraPivot.MakeParentOf(camera);
		
		UpdateManager.Add(new TestUpdater("test", world) {
			A = t3d1,
			B = t3d2,
			C = t3d3,
			D = t3d4,
			E = t3d5,
			CameraPivot = cameraPivot
		});
		
		VISIO.Run();
	}
	
	private class TestUpdater : WorldUpdater {
		
		public required Entity A { get; set; }
		public required Entity B { get; set; }
		public required Entity C { get; set; }
		public required Entity D { get; set; }
		public required Entity E { get; set; }
		public required Entity CameraPivot { get; set; }

		public TestUpdater(string id, World world) : base(id, world) { }

		public override void Update(TimeSpan delta) {
			var fd = (float) delta.TotalSeconds;
			
			// this should not affect anything as the chain is broken
			A.Get<Transform3D>().Rotate(yaw: Rotation<float>.FromDegrees(15 * fd));
			
			B.Get<Transform3D>().Translate(x: -0.7f * fd);
			
			D.Get<Transform3D>().Translate(y: 0.2f * fd);
			D.Get<Transform3D>().Rotate(yaw: Rotation<float>.FromDegrees(15 * fd));
			
			E.Get<Transform3D>().Translate(y: -0.2f * fd);
			E.Get<Transform3D>().Rotate(pitch: Rotation<float>.FromDegrees(45 * fd));
			
			CameraPivot.Get<Transform3D>().Rotate(pitch: Rotation<float>.FromDegrees(10 * fd));
		}
	}
}
