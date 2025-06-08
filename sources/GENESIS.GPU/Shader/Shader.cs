using GENESIS.GPU.OpenGL;
using Silk.NET.OpenGL;

namespace GENESIS.GPU.Shader {
	
	public abstract class Shader : IDisposable {
		
		public IPlatform Platform { get; }
		
		public ShaderType Type { get; }
		public string Code { get; }
		
		public uint Handle { get; protected set; }

		internal List<uint> DeallocatedDataObjects { get; } = [];

		protected Shader(IPlatform platform, ShaderType type, string code) {
			Platform = platform;
			Type = type;
			Code = code;
		}
		
		public abstract uint Compile();
		
		public abstract void Dispose();

		public static Shader Create(IPlatform platform, ShaderType type, string code) {
			return platform switch {
				GLPlatform glPlatform => new GLShader(glPlatform, type, code),
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}
	}
}
