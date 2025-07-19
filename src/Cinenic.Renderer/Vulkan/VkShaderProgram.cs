using System.Diagnostics;
using System.Runtime.InteropServices;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkShaderProgram : ShaderProgram {
		
		public List<PipelineShaderStageCreateInfo> Stages { get; } = [];
		public List<DescriptorSet> DescriptorSets { get; } = [];
		public List<DescriptorSetLayout> DescriptorSetLayouts { get; } = [];
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly VkPlatform _platform;

		internal DescriptorSet[] _descriptorSetsArray;

		public VkShaderProgram(VkPlatform platform, params Shader.Shader[] shaders) : base(platform, shaders) {
			_platform = platform;
		}
		
		public unsafe override void Bind(RenderPipeline pipeline) {
			Debug.Assert(pipeline is VkRenderPipeline);
			var vkPipeline = (VkRenderPipeline) pipeline;
			var vkQueue = (VkRenderQueue) pipeline.Queue;
			
			if(Handle == 0) Build();

			fixed(DescriptorSet* descriptorSetsPtr = _descriptorSetsArray) {
				_platform.API.CmdBindDescriptorSets(
					vkQueue.CommandBuffer,
					pipeline.Queue.Type switch {
						RenderQueue.Family.Graphics => PipelineBindPoint.Graphics,
						RenderQueue.Family.Compute => PipelineBindPoint.Compute,
						_ => throw new NotImplementedException()
					},
					vkPipeline.PipelineLayout,
					0,
					(uint) _descriptorSetsArray.Length,
					descriptorSetsPtr,
					0,
					null
				);
			}
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
					PName = (byte*) Marshal.StringToHGlobalAuto("main")
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

			// descriptors cannot change after program is built, so we can store the array for efficiency
			_descriptorSetsArray = DescriptorSets.ToArray();
			
			Handle = 1;
			return Handle;
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
