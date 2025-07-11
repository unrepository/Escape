using System.Diagnostics;
using System.Runtime.InteropServices;
using Cinenic.Renderer.Shader;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkShaderProgram : ShaderProgram {

		internal List<PipelineShaderStageCreateInfo> Stages { get; private set; } = [];
		
		private readonly VkPlatform _platform;

		public VkShaderProgram(VkPlatform platform, params Shader.Shader[] shaders) : base(platform, shaders) {
			_platform = platform;
		}
		
		public override void Bind() {
			if(Handle == 0) Build();
		}

		public unsafe override uint Build() {
			foreach(var shader in Shaders) {
				Debug.Assert(shader is VkShader);

				var shaderStageInfo = new PipelineShaderStageCreateInfo {
					SType = StructureType.PipelineShaderStageCreateInfo,
					Module = ((VkShader) shader).Module,
					PName = (byte*) Marshal.StringToHGlobalAuto("prog")
				};

				shaderStageInfo.Stage = shader.Type switch {
					ShaderType.FragmentShader => ShaderStageFlags.FragmentBit,
					ShaderType.VertexShader => ShaderStageFlags.VertexBit,
					ShaderType.ComputeShader => ShaderStageFlags.ComputeBit,
					ShaderType.GeometryShader => ShaderStageFlags.GeometryBit,
					ShaderType.TessControlShader => ShaderStageFlags.TessellationControlBit,
					ShaderType.TessEvaluationShader => ShaderStageFlags.TessellationEvaluationBit,
					_ => throw new ArgumentException($"Unknown shader type: {shader.Type}")
				};
				
				Stages.Add(shaderStageInfo);
			}

			Handle = 1;
			return Handle;
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
