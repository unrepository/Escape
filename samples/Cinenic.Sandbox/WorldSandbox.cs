using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Cinenic;
using Cinenic.Components;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer;
using Cinenic.Renderer.Camera;
using Cinenic.Renderer.Resources;
using Cinenic.Renderer.Shader;
using Cinenic.Renderer.Shader.Pipelines;
using Cinenic.Renderer.Vulkan;
using Cinenic.Resources;
using Cinenic.UnitTypes;
using NLog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using Camera3D = Cinenic.Components.Camera3D;
using Shader = Cinenic.Renderer.Shader.Shader;
using Texture = Cinenic.Renderer.Texture;
using Window = Cinenic.Renderer.Window;

using static Shared;
	
public static class WorldSandbox {
	
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	private static Vector2 _lastMousePosition;
	private static float _cameraPitch = 0;
	private static float _cameraYaw = 0;
	private static bool _cameraLock = false;
	
	public static void Start(string[] args) {
		SetupVulkan(out var platform, out var shaderPipeline, out var renderQueue, out var renderPipeline);
		
		var bricksAlbedo = ResourceManager.Load<TextureResource>(platform, "textures/Bricks059_1K-JPG/Bricks059_1K-JPG_Color.jpg");
		var bricksNormal = ResourceManager.Load<TextureResource>(platform, "textures/Bricks059_1K-JPG/Bricks059_1K-JPG_NormalGL.jpg");
		var bricksDisplacement = ResourceManager.Load<TextureResource>(platform, "textures/Bricks059_1K-JPG/Bricks059_1K-JPG_Displacement.jpg");
		var bricksRoughness = ResourceManager.Load<TextureResource>(platform, "textures/Bricks059_1K-JPG/Bricks059_1K-JPG_Roughness.jpg");

		var model1 = CubeModel.Clone();
		var model2 = CubeModel.Clone();
		var model3 = CubeModel.Clone();
		var model4 = CubeModel.Clone();

		model1.Meshes[0].Material.AlbedoTexture = bricksAlbedo;
		model2.Meshes[0].Material.AlbedoTexture = bricksNormal;
		model3.Meshes[0].Material.AlbedoTexture = bricksDisplacement;
		model4.Meshes[0].Material.AlbedoTexture = bricksRoughness;
		
		CreateWindow(platform, "World Sandbox", ref renderQueue, out var window);
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);
		
		_logger.Info("Create entities");
		var cube1 = world.Create(
			new Transform3D(Vector3.Zero, Quaternion.Zero, Vector3.One),
			new RenderableObject(model1)
		);
		
		var cube2 = world.Create(
			new Transform3D(new Vector3(0, 2, 0), Quaternion.Zero, Vector3.One),
			new RenderableObject(model2)
		);
		
		var cube3 = world.Create(
			new Transform3D(new Vector3(-2, 2, 0), Quaternion.Zero, Vector3.One),
			new RenderableObject(model3)
		);
		
		var cube4 = world.Create(
			new Transform3D(new Vector3(-2, 0, 0), Quaternion.Zero, Vector3.One),
			new RenderableObject(model4)
		);
		
		UpdateManager.Add(new TestRotationUpdater("test rotation", world, cube1));
		UpdateManager.Add(new TestKiller("test killer", world, cube4));
		UpdateManager.Add(new TestSwitcher("test model switcher", world, cube2, model1));
		
		_logger.Info("Create camera entity");
		var camera = world.Create(
			new Transform3D(new Vector3(-2, 0, -2), Quaternion.Zero, Vector3.One),
			new Camera3D(
				new PerspectiveCamera3D(window.Framebuffer) {
					FieldOfView = 70
				}
			)
		);
		camera.Get<Transform3D>().LookAt(Vector3.Zero);
		
		_logger.Info("Setup window input");
		Debug.Assert(window.Input is not null);
		
		window.Input.Keyboards[0].KeyDown += (keyboard, key, _) => {
			if(key == Key.Escape) {
				_cameraLock = !_cameraLock;
				if(_cameraLock) {
					_lastMousePosition = window.Input.Mice[0].Position;
					window.Input.Mice[0].Cursor.CursorMode = CursorMode.Disabled;
				} else {
					window.Input.Mice[0].Cursor.CursorMode = CursorMode.Normal;
				}
			}
			if(!_cameraLock) return;
			if(key == Key.W) camera.Get<Transform3D>().Position.Z -= 1f;
			if(key == Key.S) camera.Get<Transform3D>().Position.Z += 1f;
			if(key == Key.A) camera.Get<Transform3D>().Position.X -= 1f;
			if(key == Key.D) camera.Get<Transform3D>().Position.X += 1f;
		};

		window.Input.Mice[0].MouseMove += (mouse, position) => {
			if(!_cameraLock) return;
			
			var deltaX = (position.X - _lastMousePosition.X) * 0.004f;
			var deltaY = (position.Y - _lastMousePosition.Y) * 0.004f;
			_lastMousePosition = position;

			_cameraYaw -= deltaX;
			_cameraPitch -= deltaY;

			_cameraPitch = Math.Clamp(_cameraPitch, -89.9f.ToRadians(), 89.9f.ToRadians());
			
			camera.Get<Transform3D>().Rotation = Quaternion.CreateFromYawPitchRoll(_cameraYaw, _cameraPitch, 0);
			//Console.WriteLine(camera.Get<Transform3D>().Rotation);
		};
		
		_logger.Info("Begin loop");
		CINENIC.Run();
	}

	private class TestRotationUpdater : WorldUpdater {

		private TimeSpan _time = TimeSpan.Zero;
		private Entity _entity;

		public TestRotationUpdater(string id, World world, Entity entity) : base(id, world) {
			_entity = entity;
		}

		public override void Update(TimeSpan delta) {
			_time += delta;

			float t = (float) _time.TotalSeconds * 1f;
			
			/*_entity.GetMut<Transform3D>().RotationRadians = new Vector3(
				0 /*MathF.Sin(t)#1#,
				MathF.Cos(t) * MathF.PI * 0.5f,
				0 /*MathF.Sin(t * 0.5f)#1#
			);*/
			
			_entity.Get<Transform3D>().Yaw = Rotation<float>.FromRadians(MathF.Cos(t) * MathF.PI * 0.5f);
		}
	}
	
	private class TestKiller : WorldUpdater {

		private TimeSpan _time = TimeSpan.Zero;
		private Entity _entity;
		private bool _done;

		public TestKiller(string id, World world, Entity entity) : base(id, world) {
			_entity = entity;
		}

		public override void Update(TimeSpan delta) {
			if(_done) return;
			
			_time += delta;

			if(_time.TotalSeconds > 2.0) {
				World.Destroy(_entity);
				_done = true;
			}
		}
	}
	
	private class TestSwitcher : WorldUpdater {

		private TimeSpan _time = TimeSpan.Zero;
		private Entity _entity;
		private Model _newModel;
		private bool _done;

		public TestSwitcher(string id, World world, Entity entity, Model newModel) : base(id, world) {
			_entity = entity;
			_newModel = newModel;
		}

		public override void Update(TimeSpan delta) {
			if(_done) return;
			
			_time += delta;

			if(_time.TotalSeconds > 3.0) {
				_entity.Set(new RenderableObject(_newModel));
				_done = true;
			}
		}
	}
}
