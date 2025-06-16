using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Xml;
using Eclair.Renderer;
using Eclair.Renderer.Compute;
using Eclair.Renderer.OpenGL;
using Eclair.Renderer.Shader;
using GENESIS.LanguageExtensions;
using Eclair.Presentation;
using Eclair.Presentation.Camera;
using Eclair.Presentation.Extensions;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Framebuffer = Eclair.Renderer.Framebuffer;
using Monitor = Silk.NET.Windowing.Monitor;
using Shader = Eclair.Renderer.Shader.Shader;
using Window = Eclair.Renderer.Window;

namespace GENESIS.Sandbox {
	
	public static class ShaderComputeDemoProgram {

		public static void Start(string[] args) {
			Silk.NET.Windowing.Window.PrioritizeGlfw();

			var platformOptions = new GLPlatform.Options();
			platformOptions.ParseCommandLine(args);

			var platform = new GLPlatform(platformOptions);

			_ = Window.Create(platform);
			platform.Initialize();

			var shaderProgram = ShaderProgram.Create(
				platform,
				Shader.Create(
					platform,
					ShaderType.FragmentShader,
					Assembly
						.GetExecutingAssembly()
						.ReadTextResourceN("Shaders.demo2.frag")
				)
			);

			var shaderCompute = new ShaderCompute(platform, shaderProgram, new Vector2D<uint>(1024, 1024));
			shaderCompute.Render();
			
			var output = shaderCompute.ReadOutput();

			using(Image<Rgba32> image = new(shaderCompute.Size.X, shaderCompute.Size.Y)) {
				for(int y = 0; y < shaderCompute.Size.Y; y++) {
					for(int x = 0; x < shaderCompute.Size.X; x++) {
						image[x, y] = output[y * shaderCompute.Size.X + x];
					}
				}
				
				image.Save("demo_output.png");
			}
			
			Console.WriteLine("Saved output framebuffer to image");
		}
	}
}
