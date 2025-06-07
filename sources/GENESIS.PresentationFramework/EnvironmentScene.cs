using System.Reflection;
using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using GENESIS.PresentationFramework.Camera;
using GENESIS.PresentationFramework.Drawing;
using GENESIS.PresentationFramework.Drawing.OpenGL;
using Silk.NET.OpenGL;

namespace GENESIS.PresentationFramework {
	
	public abstract class EnvironmentScene : Scene {
		
		public IShaderProgram ShaderProgram { get; protected set; }
		public CameraBase? Camera { get; protected set; }

		protected IShader PrimaryShader => ShaderProgram.Shaders[0];

		public EnvironmentScene(GLPlatform platform, Type type, string id) : base(default, id) {
			ShaderProgram = type switch {
				Type.ThreeDimensional => IShaderProgram.Create(platform, GetDefault3DShaderSet(platform)),
				_ => throw new NotImplementedException()
			};

			Painter = new Painter {
				XYZ = new GLPainter3D(platform, ShaderProgram, PrimaryShader)
			};
		}

		protected override void Paint(double delta) {
			ShaderProgram.Bind();
			
			Camera?.Update();
			Painter.XYZ.Paint();
		}

		public static GLShader[] GetDefault3DShaderSet(GLPlatform platform)
			=> [
				new GLShader(platform, ShaderType.VertexShader,
					Assembly.GetExecutingAssembly().ReadTextResource(
						"GENESIS.PresentationFramework.Resources.Shaders.OpenGL.environment3d.vert"
				)),
				new GLShader(platform, ShaderType.FragmentShader,
					Assembly.GetExecutingAssembly().ReadTextResource(
						"GENESIS.PresentationFramework.Resources.Shaders.OpenGL.environment3d.frag"
				))
			];

		public enum Type {
			
			TwoDimensional,
			ThreeDimensional
		}
	}
}
