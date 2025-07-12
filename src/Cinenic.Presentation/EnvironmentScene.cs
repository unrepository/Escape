using System.Reflection;
using Cinenic.Renderer;
using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Shader;
using Cinenic.Extensions.CSharp;
using Cinenic.Presentation.Camera;
using Cinenic.Presentation.Drawing.OpenGL;
using Cinenic.Presentation.Drawing;
using Silk.NET.OpenGL;
using Shader = Cinenic.Renderer.Shader.Shader;

namespace Cinenic.Presentation {
	
	public abstract class EnvironmentScene : Scene {
		
		public ShaderProgram ShaderProgram { get; protected set; }
		public CameraBase? Camera { get; protected set; }

		protected Shader PrimaryShader => ShaderProgram.Shaders[0];
		
		public EnvironmentScene(IPlatform platform, string id) : base(default, id) {
			ShaderProgram = ShaderProgram.Create(platform, GetDefaultShaderSet(platform));

			Painter = platform switch {
				GLPlatform glPlatform => new GLPainter(glPlatform),
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}

		protected override void Paint(double delta) {
			ShaderProgram.Bind();
			
			Camera?.Update();
			Painter.Paint();
		}

		public static Shader[] GetDefaultShaderSet(IPlatform platform) {
			return platform switch {
				GLPlatform glPlatform => [
					new GLShader(glPlatform, Shader.Family.Vertex,
						Assembly.GetExecutingAssembly().ReadTextResource(
							"Cinenic.Presentation.Resources.Shaders.OpenGL.environment.vert"
						)),
					new GLShader(glPlatform, Shader.Family.Fragment,
						Assembly.GetExecutingAssembly().ReadTextResource(
							"Cinenic.Presentation.Resources.Shaders.OpenGL.environment.frag"
						))
				],
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}
	}
}
