using System.Reflection;
using Eclair.Presentation.Camera;
using Eclair.Presentation.Drawing.OpenGL;
using Eclair.Renderer;
using Eclair.Renderer.OpenGL;
using Eclair.Renderer.Shader;
using GENESIS.LanguageExtensions;
using Eclair.Presentation.Drawing;
using Silk.NET.OpenGL;
using Shader = Eclair.Renderer.Shader.Shader;

namespace Eclair.Presentation {
	
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
					new GLShader(glPlatform, ShaderType.VertexShader,
						Assembly.GetExecutingAssembly().ReadTextResource(
							"Eclair.Presentation.Resources.Shaders.OpenGL.environment.vert"
						)),
					new GLShader(glPlatform, ShaderType.FragmentShader,
						Assembly.GetExecutingAssembly().ReadTextResource(
							"Eclair.Presentation.Resources.Shaders.OpenGL.environment.frag"
						))
				],
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}
	}
}
