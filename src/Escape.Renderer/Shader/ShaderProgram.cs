using Escape.Renderer.OpenGL;
using Escape.Renderer.Vulkan;

namespace Escape.Renderer.Shader {
	
	public abstract class ShaderProgram : IDisposable {
		
		public IPlatform Platform { get; }
		
		public Shader[] Shaders { get; }
		public uint Handle { get; protected set; }

		protected ShaderProgram(IPlatform platform, params Shader[] shaders) {
			Platform = platform;
			Shaders = shaders;
		}

		public abstract void Bind(RenderPipeline pipeline);
		public abstract uint Build();

		public abstract void Dispose();

		public static ShaderProgram Create(IPlatform platform, params Shader[] shaders) {
			return platform switch {
				GLPlatform glPlatform => new GLShaderProgram(glPlatform, shaders),
				VkPlatform vkPlatform => new VkShaderProgram(vkPlatform, shaders),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
	}
}
