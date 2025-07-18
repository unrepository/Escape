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
using Window = Cinenic.Renderer.Window;

	
public static class WorldSandbox {
	
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	private static readonly RenderableModel _cubeModel = new RenderableModel {
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
					Albedo = Color.White.ToVector4(),
					Metallic = 0.5f,
					Roughness = 0.5f,
					UseTextures = Material.TextureType.None
				},
				Textures = []
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
		
		_logger.Info("Create shader pipeline");
		var shaderPipeline = new DefaultSceneShaderPipeline(platform);
		
		_logger.Info("Create queues");
		var queue = RenderQueueManager.Create(platform, "world");
		
		_logger.Info("Create render pipeline");
		var pipeline = RenderPipelineManager.Create(platform, "main", queue, shaderPipeline.Program);
		
		_logger.Info("Create window");
		var window = Window.Create(platform, WindowOptions.DefaultVulkan);
		window.Title = "Sandbox";
		window.Initialize(queue);
		queue.RenderTarget = window.Framebuffer;
		
		UpdateManager.Add((WindowUpdater) window);
		
		_logger.Info("Create world");
		using var world = World.Create();
		
		_logger.Info("Create renderable entity");
		world
			.Entity("renderable cube")
			.Set(new Transform3D(Vector3.Zero, Quaternion.Zero, Vector3.One))
			.Set(new Renderable(delta => {
				return _cubeModel;
			}));
		
		_logger.Info("Create camera entity");
		var camera = world
			.Entity("camera")
			.Set(new Transform3D(new Vector3(-2, 2, -2), Quaternion.Zero, Vector3.One))
			.Set(new Camera3D(
				new PerspectiveCamera3D(window.Framebuffer) {
					FieldOfView = 70
				}
			));

		world.Set(default(flecs.EcsRest));

		_logger.Info("Add object & world renderer");
		var or = ObjectRenderer.Create(platform, shaderPipeline);
		RenderManager.Add(queue, or);
		RenderManager.Add(queue, new WorldRenderer("main", or, world));
		UpdateManager.Add(new WorldUpdater("main", world));
		
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
			_cameraPitch += deltaY;

			_cameraPitch = Math.Clamp(_cameraPitch, -89.9f, 89.9f);
			
			camera.GetMut<Transform3D>().Rotation = Quaternion.CreateFromYawPitchRoll(_cameraYaw, _cameraPitch, 0);
		};
		
		_logger.Info("Begin loop");
		CINENIC.Run();
	}
}
