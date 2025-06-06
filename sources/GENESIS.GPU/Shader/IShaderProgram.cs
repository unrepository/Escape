using GENESIS.GPU.OpenGL;

namespace GENESIS.GPU.Shader {
	
	public interface IShaderProgram : IDisposable {
		
		protected IShader[] Shaders { get; }
		
		public uint Id { get; }

		public void Bind();
		public uint Build();
		public void Dispose();

		public static GLShaderProgram Create(GLPlatform platform, params IShader[] shaders)
			=> new GLShaderProgram(platform, shaders);
	}
}
