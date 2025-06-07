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

		[Obsolete]
		public EnvironmentScene(GLPlatform platform, Type type, string id) : base(default, id) {
			ShaderProgram = type switch {
				Type.ThreeDimensional => IShaderProgram.Create(platform, GetDefaultShaderSet(platform)),
				_ => throw new NotImplementedException()
			};

			Painter = new Painter {
				XY = new GLPainter2D(platform, ShaderProgram, PrimaryShader),
				XYZ = new GLPainter3D(platform, ShaderProgram, PrimaryShader)
			};
		}
		
		public EnvironmentScene(GLPlatform platform, string id) : base(default, id) {
			ShaderProgram = IShaderProgram.Create(platform, GetDefaultShaderSet(platform));

			Painter = new Painter {
				XY = new GLPainter2D(platform, ShaderProgram, PrimaryShader),
				XYZ = new GLPainter3D(platform, ShaderProgram, PrimaryShader)
			};
		}

		protected override void Paint(double delta) {
			ShaderProgram.Bind();
			
			Camera?.Update();
			Painter.XYZ.Paint();
			Painter.XY.Paint(); // TODO merging 2D and 3D painters would also mean that we can easily manipulate the drawing order
		}

		public static GLShader[] GetDefaultShaderSet(GLPlatform platform)
			=> [
				new GLShader(platform, ShaderType.VertexShader,
					Assembly.GetExecutingAssembly().ReadTextResource(
						"GENESIS.PresentationFramework.Resources.Shaders.OpenGL.environment.vert"
				)),
				new GLShader(platform, ShaderType.FragmentShader,
					Assembly.GetExecutingAssembly().ReadTextResource(
						"GENESIS.PresentationFramework.Resources.Shaders.OpenGL.environment.frag"
				))
			];

		public enum Type {
			
			TwoDimensional,
			ThreeDimensional
		}
	}
}
