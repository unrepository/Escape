using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Xml;
using Visio.Renderer;
using Visio.Compute;
using Visio.Renderer.OpenGL;
using Visio.Renderer.Shader;
using Visio.Extensions.CSharp;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Framebuffer = Visio.Renderer.Framebuffer;
using Monitor = Silk.NET.Windowing.Monitor;
using Shader = Visio.Renderer.Shader.Shader;
using Window = Visio.Renderer.Window;

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
				Shader.Family.Fragment,
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
