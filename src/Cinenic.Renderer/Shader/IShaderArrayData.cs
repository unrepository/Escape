using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Vulkan;

namespace Cinenic.Renderer.Shader {

	public interface IShaderArrayData<T> : IShaderData<T[]> {
		
		public unsafe void Write(uint index, T[] data) {
			if(Data is null) {
				Data = new T[index + data.Length];
			}

			if(Data.Length < index + data.Length) {
				var nData = new T[index + data.Length];
				Array.Copy(Data, 0, nData, 0, Data.Length);
				Data = nData;
			}

			uint memorySize = (uint) (index * sizeof(T) + data.Length * sizeof(T));
			if(Size < memorySize) {
				Size = memorySize;
			}
			
			Array.Copy(data, 0, Data, index, data.Length);
		}
	}
	
	public static class IShaderArrayData {
		
		[Obsolete]
		public static IShaderArrayData<T> Create<T>(IPlatform platform, uint binding, T[]? data, uint size) {
			return platform switch {
				GLPlatform glPlatform => new GLShaderArrayData<T>(glPlatform) {
					Binding = binding,
					Data = data,
					Size = size
				},
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
		
		public unsafe static IShaderArrayData<T> Create<T>(IPlatform platform, ShaderProgram program, uint binding, T[]? data, uint? size = null) {
			uint realSize = size ?? (uint) ((data?.Length ?? 0) * sizeof(T));
			
			return platform switch {
				GLPlatform glPlatform => new GLShaderArrayData<T>(glPlatform) {
					Binding = binding,
					Data = data,
					Size = realSize
				},
				VkPlatform vkPlatform => new VkShaderArrayData<T>(vkPlatform, program, binding, data, realSize),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
	}
}
