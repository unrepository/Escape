using System.Drawing;
using System.Numerics;
using Cinenic.Renderer;
using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Shader;
using Cinenic.Extensions.CSharp;
using Cinenic.Presentation;
using Cinenic.Presentation.Camera;
using Cinenic.Presentation.Drawing;
using Cinenic.Presentation.Extensions;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Monitor = Silk.NET.Windowing.Monitor;
using Window = Cinenic.Renderer.Window;

namespace Cinenic.Sandbox {

	public static class StaticDynamicTestProgram {

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
			
			var scene = new StaticDynamicTest(platform);
			window.PushScene(scene);

			window.PushScene(new DebugScene(platform));

			while(!window.Base.IsClosing) {
				window.RenderFrame(_ => {
					platform.API.Enable(EnableCap.DepthTest);
					platform.API.Enable(EnableCap.CullFace);
				});
			}
		}
	}
	
	public class StaticDynamicTest : EnvironmentScene {
		
		private OrbitCamera3D _orbitCamera;
		
		public StaticDynamicTest(IPlatform platform) : base(platform, "static_dynamic_test") { }

		public override void Initialize(Window window) {
			base.Initialize(window);

			Camera = new PerspectiveCamera3D(window, PrimaryShader);
			Camera.FieldOfView = 60;

			var c3d = (Camera3D) Camera;

			_orbitCamera = new OrbitCamera3D(c3d, window);
			
			Painter.BeginDrawList();
			Painter.Add2DQuad(new Vector2(0, 0), 45, new Vector2(5, 5), Color.BlueViolet);
			Painter.EndDrawList();
			
			Painter.BeginDrawList();
			Painter.Add3DCube(new Vector3(0, 2, 0), Vector3.Zero, Vector3.One, Color.Red);
			Painter.EndDrawList();
		}
		
		public override void Update(double delta) {
			base.Update(delta);
			
			_orbitCamera.Update(delta);
		}

		protected override void Paint(double delta) {
			Painter.RemoveDrawList(2);
			
			Painter.BeginDrawList();
			Painter.Add3DCube(
				new Vector3(4 * MathF.Sin((float) Window.Base.Time), 0, 4* MathF.Cos((float) Window.Base.Time)),
				Vector3.Zero,
				Vector3.One, 
				Color.Yellow
			);
			Painter.EndDrawList();
			
			base.Paint(delta);
		}
	}
}
