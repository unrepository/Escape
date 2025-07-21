using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Cinenic;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer;
using Cinenic.Renderer.Camera;
using Cinenic.Renderer.Shader;
using Cinenic.Renderer.Shader.Pipelines;
using Cinenic.Renderer.Vulkan;
using Cinenic.UnitTypes;
using Cinenic.World;
using Cinenic.World.Components;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using NLog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using Camera3D = Cinenic.World.Components.Camera3D;
using Shader = Cinenic.Renderer.Shader.Shader;
using Texture = Cinenic.Renderer.Texture;
using Window = Cinenic.Renderer.Window;

	
public static class WorldSandbox {
	
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	private static readonly Model _cubeModel = new Model {
		Meshes = [
			new Mesh {
				Vertices = [
					new() { Position = new(-0.5f,  0.5f,  0.5f), UV = new(0, 1) },
					new() { Position = new(-0.5f, -0.5f,  0.5f), UV = new(0, 0) },
					new() { Position = new( 0.5f, -0.5f,  0.5f), UV = new(1, 0) },
					new() { Position = new( 0.5f,  0.5f,  0.5f), UV = new(1, 1) },
					new() { Position = new( 0.5f,  0.5f,  0.5f), UV = new(0, 1) },
					new() { Position = new( 0.5f, -0.5f,  0.5f), UV = new(0, 0) },
					new() { Position = new( 0.5f, -0.5f, -0.5f), UV = new(1, 0) },
					new() { Position = new( 0.5f,  0.5f, -0.5f), UV = new(1, 1) },
					new() { Position = new( 0.5f,  0.5f, -0.5f), UV = new(0, 1) },
					new() { Position = new( 0.5f, -0.5f, -0.5f), UV = new(0, 0) },
					new() { Position = new(-0.5f, -0.5f, -0.5f), UV = new(1, 0) },
					new() { Position = new(-0.5f,  0.5f, -0.5f), UV = new(1, 1) },
					new() { Position = new(-0.5f,  0.5f, -0.5f), UV = new(0, 1) },
					new() { Position = new(-0.5f, -0.5f, -0.5f), UV = new(0, 0) },
					new() { Position = new(-0.5f, -0.5f,  0.5f), UV = new(1, 0) },
					new() { Position = new(-0.5f,  0.5f,  0.5f), UV = new(1, 1) },
					new() { Position = new(-0.5f,  0.5f, -0.5f), UV = new(0, 0) },
					new() { Position = new(-0.5f,  0.5f,  0.5f), UV = new(0, 1) },
					new() { Position = new( 0.5f,  0.5f,  0.5f), UV = new(1, 1) },
					new() { Position = new( 0.5f,  0.5f, -0.5f), UV = new(1, 0) },
					new() { Position = new(-0.5f, -0.5f,  0.5f), UV = new(0, 0) },
					new() { Position = new(-0.5f, -0.5f, -0.5f), UV = new(0, 1) },
					new() { Position = new( 0.5f, -0.5f, -0.5f), UV = new(1, 1) },
					new() { Position = new( 0.5f, -0.5f,  0.5f), UV = new(1, 0) },
				],
				Indices = [
					0, 1, 2, 0, 2, 3,
					4, 5, 6, 4, 6, 7,
					8, 9,10, 8,10,11,
					12, 13, 14, 12, 14, 15,
					16, 17, 18, 16, 18, 19,
					20, 21, 22, 20, 22, 23
				],
				Material = new Material {
					AlbedoColor = Color.White
				}
			}
		]
	};

	private static Vector2 _lastMousePosition;
	private static float _cameraPitch = 0;
	private static float _cameraYaw = 0;
	private static bool _cameraLock = false;
	
	public static void Start(string[] args) {
		var platform = new VkPlatform(new VkPlatform.Options {
			Headless = false
		});
		platform.Initialize();

		foreach(var device in platform.GetDevices()) {
			_logger.Info("Detected device: {name}", device.Name);
		}
		
		var primaryDevice = platform.CreateDevice(0);
		platform.PrimaryDevice = primaryDevice;
		_logger.Info("Using primary device 0 ({name})", primaryDevice.Name);
		
		_logger.Info("Create textures & models");
		var bricksAlbedo = Texture.Create(platform, Resources.Get("Textures.Bricks059_1K_JPG.Bricks059_1K-JPG_Color.jpg"));
		var bricksNormal = Texture.Create(platform, Resources.Get("Textures.Bricks059_1K_JPG.Bricks059_1K-JPG_NormalGL.jpg"));
		var bricksDisplacement = Texture.Create(platform, Resources.Get("Textures.Bricks059_1K_JPG.Bricks059_1K-JPG_Displacement.jpg"));
		var bricksRoughness = Texture.Create(platform, Resources.Get("Textures.Bricks059_1K_JPG.Bricks059_1K-JPG_Roughness.jpg"));

		var model1 = _cubeModel.Clone();
		var model2 = _cubeModel.Clone();
		var model3 = _cubeModel.Clone();
		var model4 = _cubeModel.Clone();

		// model1.Meshes[0].Material.AlbedoTexture = bricksAlbedo;
		// model2.Meshes[0].Material.AlbedoTexture = bricksNormal;
		// model3.Meshes[0].Material.AlbedoTexture = bricksDisplacement;
		// model4.Meshes[0].Material.AlbedoTexture = bricksRoughness;
		model1.Meshes[0].Material.AlbedoColor = Color.Yellow;
		model2.Meshes[0].Material.AlbedoColor = Color.Red;
		model3.Meshes[0].Material.AlbedoColor = Color.Green;
		model4.Meshes[0].Material.AlbedoColor = Color.Blue;
		
		_logger.Info("Create shader pipeline");
		var shaderPipeline = new DefaultSceneShaderPipeline(platform);
		
		_logger.Info("Create queues");
		var queue = RenderQueueManager.Create(platform, "world");
		
		_logger.Info("Create render pipeline");
		var pipeline = RenderPipelineManager.Create(platform, "main", queue, shaderPipeline);
		
		_logger.Info("Create window");
		var window = Window.Create(platform, WindowOptions.DefaultVulkan);
		window.Title = "Sandbox";
		window.Initialize(queue);
		queue.RenderTarget = window.Framebuffer;
		
		UpdateManager.Add((WindowUpdater) window);
		
		_logger.Info("Create world");
		using var world = World.Create();
		
		_logger.Info("Add object & world renderer");
		var or = ObjectRenderer.Create(platform, shaderPipeline);
		RenderManager.Add(queue, or);
		RenderManager.Add(queue, new WorldRenderer("main", or, world));
		UpdateManager.Add(new WorldUpdater("main", world));
		
		_logger.Info("Create renderable entities");
		var cube1 = world
			.Entity("cube 1")
			.Set(new Transform3D(Vector3.Zero, Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model1));
		
		var cube2 = world
			.Entity("cube 2")
			.Set(new Transform3D(new Vector3(0, 2, 0), Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model2));
		
		var cube3 = world
			.Entity("cube 3")
			.Set(new Transform3D(new Vector3(-2, 2, 0), Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model3));
		
		var cube4 = world
			.Entity("cube 4")
			.Set(new Transform3D(new Vector3(-2, 0, 0), Quaternion.Zero, Vector3.One))
			.Set(new RenderableObject(model4));
		
		UpdateManager.Add(new TestRotationUpdater("test rotation", world, cube1));
		UpdateManager.Add(new TestKiller("test killer", world, cube4));
		UpdateManager.Add(new TestSwitcher("test model switcher", world, cube2, model1));
		
		_logger.Info("Create camera entity");
		var camera = world
			.Entity("camera")
			.Set(new Transform3D(new Vector3(-2, 0, -2), Quaternion.Zero, Vector3.One))
			.Set(new Camera3D(
				new PerspectiveCamera3D(window.Framebuffer) {
					FieldOfView = 70
				}
			));
		camera.GetMut<Transform3D>().LookAt(Vector3.Zero);

		//world.Set(default(flecs.EcsRest));
		
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
			if(key == Key.W) camera.GetMut<Transform3D>().Position.Z -= 1f;
			if(key == Key.S) camera.GetMut<Transform3D>().Position.Z += 1f;
			if(key == Key.A) camera.GetMut<Transform3D>().Position.X -= 1f;
			if(key == Key.D) camera.GetMut<Transform3D>().Position.X += 1f;
		};

		window.Input.Mice[0].MouseMove += (mouse, position) => {
			if(!_cameraLock) return;
			
			var deltaX = (position.X - _lastMousePosition.X) * 0.004f;
			var deltaY = (position.Y - _lastMousePosition.Y) * 0.004f;
			_lastMousePosition = position;

			_cameraYaw -= deltaX;
			_cameraPitch -= deltaY;

			_cameraPitch = Math.Clamp(_cameraPitch, -89.9f.ToRadians(), 89.9f.ToRadians());
			
			camera.GetMut<Transform3D>().Rotation = Quaternion.CreateFromYawPitchRoll(_cameraYaw, _cameraPitch, 0);
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

			_entity.GetMut<Transform3D>().Yaw = Rotation<float>.FromRadians(MathF.Cos(t) * MathF.PI * 0.5f);
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
				_entity.Destruct();
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
