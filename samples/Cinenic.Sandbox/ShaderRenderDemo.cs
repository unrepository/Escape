using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Xml;
using Cinenic.Renderer;
using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Shader;
using Cinenic.Extensions.CSharp;
using Cinenic.Presentation;
using Cinenic.Presentation.Camera;
using Cinenic.Presentation.Extensions;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Framebuffer = Cinenic.Renderer.Framebuffer;
using Monitor = Silk.NET.Windowing.Monitor;
using Shader = Cinenic.Renderer.Shader.Shader;
using Window = Cinenic.Renderer.Window;

namespace Cinenic.Sandbox {
	
	public static class ShaderRenderDemoProgram {

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
			
			var scene = new ShaderRenderDemo(platform);
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

	public class ShaderRenderDemo : EnvironmentScene {

		private readonly Vector2D<uint> _framebufferSize = new Vector2D<uint>(512, 512);
		private readonly Framebuffer _framebuffer;

		public ShaderRenderDemo(IPlatform platform) : base(platform, "shader_render") {
			ShaderProgram.Shaders[1] = Shader.Create(platform, Shader.Family.Fragment,
				Assembly.GetExecutingAssembly().ReadTextResource("Cinenic.Sandbox.Resources.Shaders.demo.frag"));
			
			_framebuffer = Framebuffer.Create(platform, _framebufferSize);
		}

		public override void Initialize(Window window) {
			base.Initialize(window);

			Camera = new Camera2D((int) _framebufferSize.X, (int) _framebufferSize.Y, PrimaryShader);
			
			Painter.BeginDrawList();
			Painter.Add2DQuad(Vector2.Zero, 0, new Vector2(_framebufferSize.X, _framebufferSize.Y), Color.Red);
			Painter.EndDrawList();
		}

		protected override void Paint(double delta) {
			_framebuffer.Bind();
			base.Paint(delta);
			_framebuffer.Unbind();
		}
	}
}
