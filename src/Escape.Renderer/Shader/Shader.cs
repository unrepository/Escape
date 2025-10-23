using Escape.Renderer.OpenGL;
using Escape.Renderer.Vulkan;
using Silk.NET.OpenGL;

namespace Escape.Renderer.Shader {
	
	public abstract class Shader : IDisposable {
		
		public IPlatform Platform { get; }
		
		public Family Type { get; }
		public string Code { get; }
		
		public ulong Handle { get; protected set; }

		internal List<uint> DeallocatedDataObjects { get; } = [];

		protected Shader(IPlatform platform, Family type, string code) {
			Platform = platform;
			Type = type;
			Code = code;
		}
		
		public abstract ulong Compile();
		public abstract void Dispose();

		public static Shader Create(IPlatform platform, Family type, string code) {
			return platform switch {
				GLPlatform glPlatform => new GLShader(glPlatform, type, code),
				VkPlatform vkPlatform => new VkShader(vkPlatform, type, code),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}

		public enum Family {
			
			Vertex,
			Fragment,
			Compute,
			Geometry,
			TessellationControl,
			TessellationEvaluation
		}
	}
}
