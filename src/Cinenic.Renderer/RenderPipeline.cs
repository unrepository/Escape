using System.Numerics;
using Cinenic.Renderer.Shader;

namespace Cinenic.Renderer {
	
	public abstract class RenderPipeline : IDisposable {

		public IPlatform Platform { get; }
		
		public RenderQueue Queue { get; }
		public ShaderProgram Program { get; }

		public RenderPipeline(IPlatform platform, RenderQueue queue, ShaderProgram program) {
			Platform = platform;
			Queue = queue;
			Program = program;
		}
		
		public abstract void Dispose();

		// public static RenderPipeline Create(IPlatform platform, RenderQueue queue, ShaderProgram program) {
		// 	return platform switch {
		// 		_ => throw new NotImplementedException() // Platform_Impl
		// 	};
		// }

		public enum Family {
			
			Graphics,
			Compute,
		}
	}
}
