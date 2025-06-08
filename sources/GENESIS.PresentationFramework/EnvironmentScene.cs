using System.Reflection;
using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using GENESIS.PresentationFramework.Camera;
using GENESIS.PresentationFramework.Drawing;
using GENESIS.PresentationFramework.Drawing.OpenGL;
using Silk.NET.OpenGL;
using Shader = GENESIS.GPU.Shader.Shader;

namespace GENESIS.PresentationFramework {
	
	public abstract class EnvironmentScene : Scene {
		
		public ShaderProgram ShaderProgram { get; protected set; }
		public CameraBase? Camera { get; protected set; }

		protected Shader PrimaryShader => ShaderProgram.Shaders[0];
		
		public EnvironmentScene(IPlatform platform, string id) : base(default, id) {
			ShaderProgram = ShaderProgram.Create(platform, GetDefaultShaderSet(platform));

			Painter = platform switch {
				GLPlatform glPlatform => new GLPainter(glPlatform, ShaderProgram, PrimaryShader),
				_ => throw new NotImplementedException()
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
							"GENESIS.PresentationFramework.Resources.Shaders.OpenGL.environment.vert"
						)),
					new GLShader(glPlatform, ShaderType.FragmentShader,
						Assembly.GetExecutingAssembly().ReadTextResource(
							"GENESIS.PresentationFramework.Resources.Shaders.OpenGL.environment.frag"
						))
				],
				_ => throw new NotImplementedException()
			};
		}
	}
}
