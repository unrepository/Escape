using Eclair.Renderer.OpenGL;

namespace Eclair.Renderer.Shader {

	public interface IShaderArrayData<T> : IShaderData<T[]>;
	
	public static class IShaderArrayData {
		
		public static IShaderArrayData<T> Create<T>(IPlatform platform, uint binding, T[]? data, uint size) {
			return platform switch {
				GLPlatform glPlatform => new GLShaderArrayData<T>(glPlatform) {
					Binding = binding,
					Data = data,
					Size = size
				},
				_ => throw new NotImplementedException() // PlatformImpl
			};
		}
	}
}
