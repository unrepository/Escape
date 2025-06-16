using Eclair.Renderer.OpenGL;

namespace Eclair.Renderer.Shader {
	
	public interface IShaderData<T> : IDisposable {
		
		public uint Handle { get; }
		
		public uint Binding { get; init; }
		
		public T? Data { get; set; }
		public uint Size { get; set; }

		public void Push();
		public void Read();
	}

	public static class IShaderData {
		
		public static IShaderData<T> Create<T>(IPlatform platform, uint binding, T? data, uint size) {
			return platform switch {
				GLPlatform glPlatform => new GLShaderData<T>(glPlatform) {
					Binding = binding,
					Data = data,
					Size = size
				},
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}
	}
}
