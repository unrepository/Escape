using System.Drawing;
using System.Numerics;
using System.Reflection;
using Eclair.Renderer;
using Eclair.Renderer.OpenGL;
using Eclair.Renderer.Shader;
using Eclair.Extensions.CSharp;
using Eclair.Presentation;
using Eclair.Presentation.Camera;
using Eclair.Presentation.Drawing;
using Eclair.Presentation.Extensions;
using Eclair.Presentation.Extensions.ModelLoading;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Monitor = Silk.NET.Windowing.Monitor;
using Texture = Eclair.Renderer.Texture;
using Window = Eclair.Renderer.Window;

namespace GENESIS.Sandbox {

	public static class ModelTestProgram {

		public static void Start(string[] args) {
			Silk.NET.Windowing.Window.PrioritizeGlfw();

			var platformOptions = new GLPlatform.Options();
			platformOptions.ParseCommandLine(args);

			var platform = new GLPlatform(platformOptions);
			var windowOptions = WindowOptions.Default;

			var monitorCenter = Monitor.GetMainMonitor(null).Bounds.Center;
					
			windowOptions.Position = new Vector2D<int>(
				monitorCenter.X - windowOptions.Size.X / 2,
				monitorCenter.Y - windowOptions.Size.Y / 2
			);

			var window = Window.Create(platform, windowOptions);

			platform.Initialize();
			window.Initialize();
			
			var scene = new ModelTest(platform);
			window.PushScene(scene);

			window.PushScene(new DebugScene(platform));

			while(!window.Base.IsClosing) {
				window.RenderFrame(_ => {
					platform.API.Enable(EnableCap.DepthTest);
					//platform.API.Enable(EnableCap.CullFace);
				});
			}
		}
	}
	
	public class ModelTest : EnvironmentScene {
		
		private OrbitCamera3D _orbitCamera;

		private Model _circleModel;
		private Model _testModel;

		public ModelTest(IPlatform platform) : base(platform, "texture_test") {
			_circleModel = Models.CircleOutline(5, 3, 12);
			_testModel = AssimpModelLoader.Load(platform, "thing", Resources.LoadBinary("Models.thing.glb"));
		}

		public override void Initialize(Window window) {
			base.Initialize(window);

			Camera = new PerspectiveCamera3D(window, PrimaryShader);
			Camera.FieldOfView = 60;

			var c3d = (Camera3D) Camera;

			_orbitCamera = new OrbitCamera3D(c3d, window);
			
			Painter.BeginDrawList();
			
			Painter.Add3DObject(
				_testModel,
				new Vector3(0, 0, 0),
				Vector3.Zero,
				Vector3.One
			);
			
			Painter.Add3DObject(
				_circleModel,
				new Vector3(0, -1, 0),
				Vector3.Zero,
				Vector3.One,
				Color.BlueViolet
			);
			
			Painter.EndDrawList();
		}
		
		public override void Update(double delta) {
			base.Update(delta);
			
			_orbitCamera.Update(delta);
		}
	}
}
