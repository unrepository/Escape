using GENESIS.GPU.OpenGL;
using Silk.NET.OpenGL;

namespace GENESIS.GPU.Shader {
	
	public abstract class Shader : IDisposable {
		
		public ShaderType Type { get; }
		public string Code { get; }
		
		public uint Handle { get; protected set; }

		internal List<uint> DeallocatedDataObjects { get; } = [];

		protected Shader(ShaderType type, string code) {
			Type = type;
			Code = code;
		}
		
		public abstract uint Compile();

		public abstract void PushData<T>(ShaderData<T> data);
		public abstract void UpdateData<T>(ShaderData<T> data);
		public abstract void ReadData<T>(ref ShaderData<T> data);

		public abstract void Dispose();

		public static Shader Create(IPlatform platform, ShaderType type, string code) {
			return platform switch {
				GLPlatform glPlatform => new GLShader(glPlatform, type, code),
				_ => throw new NotImplementedException()
			};
		}
	}
}
