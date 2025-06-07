using GENESIS.GPU.OpenGL;
using Silk.NET.OpenGL;

namespace GENESIS.GPU.Shader {
	
	public interface IShader : IDisposable {
		
		public ShaderType Type { get; }
		public string Code { get; }
		
		public uint Id { get; }

		internal List<uint> DeallocatedDataObjects { get; }
		
		public uint Compile();

		public void PushData<T>(ShaderData<T> data);
		public void UpdateData<T>(ShaderData<T> data);
		public void ReadData<T>(ref ShaderData<T> data);

		/*public static IShader Create<TAPI, TDevice>(IPlatform<TAPI, TDevice> platform, ShaderType type, string code)
			where TDevice : IDevice
		{
			return platform switch {
				GLPlatform glPlatform => new GLShader(glPlatform, type, code),
				_ => throw new NotImplementedException()
			};
		}*/

		public static IShader Create(GLPlatform platform, ShaderType type, string code)
			=> new GLShader(platform, type, code);
	}
}
