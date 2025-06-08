using GENESIS.GPU.OpenGL;

namespace GENESIS.GPU.Shader {
	
	public abstract class ShaderProgram : IDisposable {
		
		public IPlatform Platform { get; }
		
		public Shader[] Shaders { get; }
		public uint Handle { get; protected set; }

		protected ShaderProgram(IPlatform platform, params Shader[] shaders) {
			Platform = platform;
			Shaders = shaders;
		}

		public abstract void Bind();
		public abstract uint Build();

		public abstract void Dispose();

		public static ShaderProgram Create(IPlatform platform, params Shader[] shaders) {
			return platform switch {
				GLPlatform glPlatform => new GLShaderProgram(glPlatform, shaders),
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}
	}
}
