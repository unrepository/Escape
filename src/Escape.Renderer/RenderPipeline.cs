using System.Numerics;
using Escape.Renderer.Resources;
using Escape.Renderer.Shader;
using Escape.Resources;

namespace Escape.Renderer {
	
	public abstract class RenderPipeline : IDisposable {

		public IPlatform Platform { get; }

		public RenderQueue Queue;
		//public Framebuffer RenderTarget { get; set; }
		public IShaderPipeline ShaderPipeline { get; }
		public Ref<ShaderProgramResource> Program => ShaderPipeline.Program;

		public RenderPipeline(IPlatform platform, RenderQueue queue, IShaderPipeline shaderPipeline) {
			Platform = platform;
			Queue = queue;
			ShaderPipeline = shaderPipeline;
			queue.Pipeline = this;
		}

		public abstract bool Begin();
		public abstract bool End();
		
		public abstract void Dispose();

		// public static RenderPipeline Create(IPlatform platform, RenderQueue queue, ShaderProgram program) {
		// 	return platform switch {
		// 		_ => throw new NotImplementedException() // Platform_Impl
		// 	};
		// }
	}
}
