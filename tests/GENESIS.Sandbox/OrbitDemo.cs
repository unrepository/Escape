using System.Drawing;
using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using GENESIS.PresentationFramework;
using GENESIS.PresentationFramework.Camera;
using GENESIS.PresentationFramework.Drawing;
using GENESIS.PresentationFramework.Extensions;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Monitor = Silk.NET.Windowing.Monitor;
using Window = GENESIS.GPU.Window;

namespace GENESIS.Sandbox {

	public static class OrbitDemoProgram {

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
			
			var scene = new OrbitDemo(platform);
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
	
	public class OrbitDemo : EnvironmentScene {

		private Random _r = new();

		private const float SPHERE_RADIUS = 256;
		private const int OBJECT_COUNT = 50000;
		private readonly List<OrbitalObject> _orbitalObjects = [];
		
		public OrbitDemo(IPlatform platform) : base(platform, "test3d") {
			for(int i = 0; i < OBJECT_COUNT; i++) {
				var pos = _r.NextSphereCoordinate(SPHERE_RADIUS);
				var obj = new OrbitalObject(pos, pos.Length(), 1, _r);

				obj.Color = Color.FromArgb(
					_r.Next(200, 255),
					_r.Next(10, 80),
					_r.Next(0, 10),
					_r.Next(100, 240)
				);
				
				_orbitalObjects.Add(obj);
			}
		}

		public override void Initialize(Window window) {
			base.Initialize(window);

			Camera = new PerspectiveCamera3D(window, PrimaryShader);
			Camera.FieldOfView = 60;

			var c3d = (Camera3D) Camera;

			_orbitCamera = new OrbitCamera3D(c3d, window);
		}
		
		private OrbitCamera3D _orbitCamera;
		
		public override void Update(double delta) {
			base.Update(delta);

			for(int i = 0; i < OBJECT_COUNT; i++) {
				_orbitalObjects[i].Rotation += new Vector3(
					_r.NextSingle() * (float) delta,
					_r.NextSingle() * (float) delta,
					_r.NextSingle() * (float) delta
				);
			}
			
			_orbitCamera.Update(delta);
		}

		protected override void Paint(double delta) {
			Painter.XYZ.Clear();
			
			Painter.XYZ.BeginDrawList();
			for(int i = 0; i < OBJECT_COUNT; i++) {
				var obj = _orbitalObjects[i];
				Painter.XYZ.AddCube(obj.GetPosition((float) Window!.Base.Time), obj.Rotation, obj.Scale, obj.Color);
			}
			Painter.XYZ.EndDrawList();
			
			base.Paint(delta);
		}

		private class OrbitalObject {

			public Vector3 InitialPosition;
			public float Radius;
			public float Speed;
			
			public Vector3 Rotation = Vector3.Zero;
			public Vector3 Scale = Vector3.One;
			public Color Color;
			
			public Vector3 Axis1;
			public Vector3 Axis2;

			public OrbitalObject(Vector3 initialPosition, float radius, float speed, Random r) {
				InitialPosition = Vector3.Normalize(initialPosition);
				Radius = radius;
				Speed = speed;

				Scale = new Vector3(5) * (1 - initialPosition.Length() / SPHERE_RADIUS);
				Color = Color.White.Randomize();

				var up = Vector3.UnitZ;
				if(MathF.Abs(Vector3.Dot(InitialPosition, up)) > 0.99f) {
					up = Vector3.UnitX;
				}

				Axis1 = Vector3.Normalize(Vector3.Cross(InitialPosition, up));
				Axis2 = Vector3.Normalize(Vector3.Cross(InitialPosition, Axis1));
			}

			public Vector3 GetPosition(float time) {
				float angle = Speed * time;
				
				return Radius * MathF.Cos(angle) * Axis1
				       + Radius * MathF.Sin(angle) * Axis2;
			}
		}
	}
}
