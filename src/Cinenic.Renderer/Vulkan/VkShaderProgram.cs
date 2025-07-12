using System.Diagnostics;
using System.Runtime.InteropServices;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkShaderProgram : ShaderProgram {

		internal List<PipelineShaderStageCreateInfo> Stages { get; private set; } = [];

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly VkPlatform _platform;

		public VkShaderProgram(VkPlatform platform, params Shader.Shader[] shaders) : base(platform, shaders) {
			_platform = platform;
		}
		
		public override void Bind() {
			if(Handle == 0) Build();
		}

		public unsafe override uint Build() {
			if(Handle != 0) return Handle;
			
			_logger.Debug("Building program with {Count} shaders", Shaders.Length);
			
			foreach(var shader in Shaders) {
				Debug.Assert(shader is VkShader);
				shader.Compile();

				var shaderStageInfo = new PipelineShaderStageCreateInfo {
					SType = StructureType.PipelineShaderStageCreateInfo,
					Module = ((VkShader) shader).Module,
					PName = (byte*) Marshal.StringToHGlobalAuto("prog")
				};

				shaderStageInfo.Stage = shader.Type switch {
					Shader.Shader.Family.Fragment => ShaderStageFlags.FragmentBit,
					Shader.Shader.Family.Vertex => ShaderStageFlags.VertexBit,
					Shader.Shader.Family.Compute => ShaderStageFlags.ComputeBit,
					Shader.Shader.Family.Geometry => ShaderStageFlags.GeometryBit,
					Shader.Shader.Family.TessellationControl => ShaderStageFlags.TessellationControlBit,
					Shader.Shader.Family.TessellationEvaluation => ShaderStageFlags.TessellationEvaluationBit
				};
				
				Stages.Add(shaderStageInfo);
			}
			
			_logger.Debug("Program built");

			Handle = 1;
			return Handle;
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
