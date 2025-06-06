using System.Reflection;
using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using GENESIS.PresentationFramework.Drawing;
using Silk.NET.OpenGL;

namespace GENESIS.PresentationFramework {
	
	public abstract class EnvironmentScene : Scene {
		
		public IShaderProgram ShaderProgram { get; set; }

		public EnvironmentScene(GLPlatform platform, Painter painter, Type type, string id) : base(painter, id) {
			switch(type) {
				case Type.ThreeDimensional:
					ShaderProgram = IShaderProgram.Create(platform, GetDefault3DShaderSet(platform));
					break;
				default:
					throw new NotImplementedException();
					break;
			}
		}

		public override void Initialize(Window window) {
			base.Initialize(window);
			
			// initialize camera
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
