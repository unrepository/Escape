using Escape.Renderer.OpenGL;
using Escape.Renderer.Vulkan;
using NLog;

namespace Escape.Renderer.Shader {
	
	public interface IShaderData<T> : IDisposable {
		
		public ulong Handle { get; }
		public uint Binding { get; }
		
		public T? Data { get; set; }
		public uint Size { get; set; }

		public bool IsDirty { get; set; }

		public void Push();
		public void Read();
	}

	public static class IShaderData {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		[Obsolete]
		public static IShaderData<T> Create<T>(IPlatform platform, uint binding, T? data, uint size) {
			return platform switch {
				GLPlatform glPlatform => new GLShaderData<T>(glPlatform) {
					Binding = binding,
					Data = data,
					Size = size
				},
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
		
		public unsafe static IShaderData<T> Create<T>(IPlatform platform, ShaderProgram program, uint binding, T? data, uint? size = null) {
			uint realSize = size ?? (uint) sizeof(T);
			
			return platform switch {
				GLPlatform glPlatform => new GLShaderData<T>(glPlatform) {
					Binding = binding,
					Data = data,
					Size = realSize
				},
				VkPlatform vkPlatform => new VkShaderData<T>(vkPlatform, program, binding, data, realSize),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
	}
}
