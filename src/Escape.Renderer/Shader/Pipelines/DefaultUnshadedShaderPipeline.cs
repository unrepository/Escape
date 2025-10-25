using Escape.Renderer.Resources;
using Escape.Resources;

namespace Escape.Renderer.Shader.Pipelines {
	
	public class DefaultUnshadedShaderPipeline : DefaultBaseShaderPipeline {

		public DefaultUnshadedShaderPipeline(IPlatform platform)
			: base(
				platform,
				ResourceManager.Load<ShaderProgramResource>(platform, "/shader_programs/unshaded.program")!
			) { }
	}
}
