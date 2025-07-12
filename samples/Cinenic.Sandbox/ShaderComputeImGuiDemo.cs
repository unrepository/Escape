using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using Cinenic.Renderer;
using Cinenic.Compute;
using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Shader;
using Cinenic.Extensions.CSharp;
using Cinenic.Presentation;
using Cinenic.Presentation.Camera;
using Cinenic.Presentation.Extensions;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Framebuffer = Cinenic.Renderer.Framebuffer;
using Monitor = Silk.NET.Windowing.Monitor;
using Shader = Cinenic.Renderer.Shader.Shader;
using Window = Cinenic.Renderer.Window;

namespace Cinenic.Sandbox {
	
	public static class ShaderComputeImGuiDemoProgram {

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
			
			window.PushScene(new ShaderComputeImGuiDemo(platform));

			while(!window.Base.IsClosing) {
				window.RenderFrame();
			}
		}
	}

	public class ShaderComputeImGuiDemo : ImGuiScene {

		private ShaderCompute _compute;
		private IShaderData<ComputeCustomDataDemo> _computeData;
		private ComputeCustomDataDemo _data;

		public ShaderComputeImGuiDemo(IPlatform platform) : base(platform, "shader-compute-imgui") {
			var computeProgram = ShaderProgram.Create(
				platform,
				Shader.Create(
					platform,
					Shader.Family.Fragment,
					Assembly
						.GetExecutingAssembly()
						.ReadTextResourceN("Shaders.demo3.frag")
				)
			);

			_compute = new ShaderCompute(platform, computeProgram, new Vector2D<uint>(512, 512));

			unsafe {
				_computeData = IShaderData.Create(
					platform,
					0,
					_data,
					(uint) sizeof(ComputeCustomDataDemo)
				);
			}
		}

		public override void Update(double delta) {
			base.Update(delta);

			_data.Time = MathF.Sin((float) Window.Base.Time) * 2;
		}

		protected override void Paint(double delta) {
			_computeData.Data = _data;
			_computeData.Push();
			
			_compute.Render();
			
			if(ImGui.Begin(Id)) {
				ImGui.Image(
					new ImTextureID(_compute.Framebuffer.GetTextureAttachments()[0].Handle),
					new Vector2(_compute.Size.X, _compute.Size.Y),
					new Vector2(0, 1), new Vector2(1, 0)
				);
			}
			
			ImGui.End();
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct ComputeCustomDataDemo() {

		[FieldOffset(0)] public float Time = 0;
	}
}
