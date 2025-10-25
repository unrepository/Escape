using Escape.Renderer.Resources;
using Escape.Resources;

namespace Escape.Renderer.Shader.Pipelines {
	
	public class DefaultPBRShaderPipeline : DefaultBaseShaderPipeline {

		public DefaultPBRShaderPipeline(IPlatform platform)
			: base(
				platform,
				ResourceManager.Load<ShaderProgramResource>(platform, "/shader_programs/pbr.program")!
			) { }
	}
}
