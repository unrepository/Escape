using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkRenderPipeline : RenderPipeline {
		
		internal readonly PipelineDynamicStateCreateInfo DynamicStateInfo;
		internal readonly PipelineVertexInputStateCreateInfo VertexInputInfo;
		internal readonly PipelineInputAssemblyStateCreateInfo InputAssemblyInfo;
		internal readonly PipelineViewportStateCreateInfo ViewportInfo;
		internal readonly PipelineRasterizationStateCreateInfo RasterizationInfo;
		internal readonly PipelineMultisampleStateCreateInfo MultisampleInfo;
		internal readonly PipelineColorBlendStateCreateInfo ColorBlendInfo;

		internal readonly PipelineLayout PipelineLayout;
		internal readonly Pipeline Pipeline;

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private static readonly DynamicState[] _dynamicStates = [
			DynamicState.Viewport,
			DynamicState.Scissor,
		];

		private readonly VkPlatform _platform;

		public unsafe VkRenderPipeline(VkPlatform platform, RenderQueue queue, ShaderProgram program)
			: base(platform, queue, program) 
		{
			_platform = platform;

		#region Pipeline layout
			_logger.Debug("Pipeline setup: Create layout");
			
			fixed(DynamicState* dynamicStatesPtr = &_dynamicStates[0]) {
				DynamicStateInfo = new PipelineDynamicStateCreateInfo {
					SType = StructureType.PipelineDynamicStateCreateInfo,
					DynamicStateCount = (uint) _dynamicStates.Length,
					PDynamicStates = dynamicStatesPtr
				};
			}

			VertexInputInfo = new PipelineVertexInputStateCreateInfo {
				SType = StructureType.PipelineVertexInputStateCreateInfo,
				VertexBindingDescriptionCount = 0,
				PVertexBindingDescriptions = null,
				VertexAttributeDescriptionCount = 0,
				PVertexAttributeDescriptions = null
			};

			InputAssemblyInfo = new PipelineInputAssemblyStateCreateInfo {
				SType = StructureType.PipelineInputAssemblyStateCreateInfo,
				Topology = PrimitiveTopology.TriangleList,
				PrimitiveRestartEnable = false
			};

			ViewportInfo = new PipelineViewportStateCreateInfo {
				SType = StructureType.PipelineViewportStateCreateInfo,
				ViewportCount = 1,
				ScissorCount = 1
			};

			RasterizationInfo = new PipelineRasterizationStateCreateInfo {
				SType = StructureType.PipelineRasterizationStateCreateInfo,
				DepthClampEnable = false,
				RasterizerDiscardEnable = false,
				PolygonMode = PolygonMode.Fill,
				LineWidth = 1.0f,
				CullMode = CullModeFlags.BackBit,
				FrontFace = FrontFace.Clockwise,
				DepthBiasEnable = false,
				DepthBiasConstantFactor = 0.0f,
				DepthBiasClamp = 0.0f,
				DepthBiasSlopeFactor = 0.0f
			};

			MultisampleInfo = new PipelineMultisampleStateCreateInfo {
				SType = StructureType.PipelineMultisampleStateCreateInfo,
				SampleShadingEnable = false,
				RasterizationSamples = SampleCountFlags.Count1Bit,
				MinSampleShading = 1.0f,
				PSampleMask = null,
				AlphaToCoverageEnable = false,
				AlphaToOneEnable = false
			};

			var colorBlendAttachment = new PipelineColorBlendAttachmentState {
				ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
				BlendEnable = false,
				SrcColorBlendFactor = BlendFactor.One,
				DstColorBlendFactor = BlendFactor.Zero,
				ColorBlendOp = BlendOp.Add,
				SrcAlphaBlendFactor = BlendFactor.One,
				DstAlphaBlendFactor = BlendFactor.Zero,
				AlphaBlendOp = BlendOp.Add
			};

			var blendConstants = new float[4] { 0, 0, 0, 0 };
			fixed(float* blendConstantsPtr = &blendConstants[0]) {
				ColorBlendInfo = new PipelineColorBlendStateCreateInfo {
					SType = StructureType.PipelineColorBlendStateCreateInfo,
					LogicOpEnable = false,
					LogicOp = LogicOp.Copy,
					AttachmentCount = 1,
					PAttachments = &colorBlendAttachment,
					BlendConstants = blendConstantsPtr
				};
			}

			var pipelineLayoutInfo = new PipelineLayoutCreateInfo {
				SType = StructureType.PipelineLayoutCreateInfo,
				SetLayoutCount = 0,
				PSetLayouts = null,
				PushConstantRangeCount = 0,
				PPushConstantRanges = null
			};

			Result result;

			fixed(PipelineLayout* pipelineLayoutPtr = &PipelineLayout) {
				if(
					(result = _platform.API.CreatePipelineLayout(
						_platform.PrimaryDevice!.Logical,
						&pipelineLayoutInfo,
						null,
						pipelineLayoutPtr)
					)
					!= Result.Success
				) {
					throw new PlatformException($"Could not create pipeline layout: {result}");
				}
			}
		#endregion

		#region Pipeline
			_logger.Debug("Pipeline setup: Create pipeline ({Family})", Queue.Type);
			Pipeline pipeline;
			
			switch(Queue.Type) {
				case RenderQueue.Family.Graphics:
					var pipelineInfo = Queue.Type switch {
						RenderQueue.Family.Graphics => new GraphicsPipelineCreateInfo {
							SType = StructureType.GraphicsPipelineCreateInfo,
							StageCount = (uint) ((VkShaderProgram) Program).Stages.Count,
							Layout = PipelineLayout,
							RenderPass = ((VkRenderQueue) Queue).Base,
							Subpass = 0,
							BasePipelineHandle = new Pipeline(),
							BasePipelineIndex = -1
						}
					};

					fixed(PipelineShaderStageCreateInfo* ptr = &((VkShaderProgram) Program).Stages.ToArray()[0]) {
						pipelineInfo.PStages = ptr;
					}
			
					fixed(PipelineDynamicStateCreateInfo* ptr = &DynamicStateInfo) {
						pipelineInfo.PDynamicState = ptr;
					}
			
					fixed(PipelineVertexInputStateCreateInfo* ptr = &VertexInputInfo) {
						pipelineInfo.PVertexInputState = ptr;
					}
			
					fixed(PipelineInputAssemblyStateCreateInfo* ptr = &InputAssemblyInfo) {
						pipelineInfo.PInputAssemblyState = ptr;
					}
			
					fixed(PipelineViewportStateCreateInfo* ptr = &ViewportInfo) {
						pipelineInfo.PViewportState = ptr;
					}
			
					fixed(PipelineRasterizationStateCreateInfo* ptr = &RasterizationInfo) {
						pipelineInfo.PRasterizationState = ptr;
					}
			
					fixed(PipelineMultisampleStateCreateInfo* ptr = &MultisampleInfo) {
						pipelineInfo.PMultisampleState = ptr;
					}
			
					fixed(PipelineColorBlendStateCreateInfo* ptr = &ColorBlendInfo) {
						pipelineInfo.PColorBlendState = ptr;
					}

					pipeline = new Pipeline();
			
					if(
						(result = _platform.API.CreateGraphicsPipelines(
							_platform.PrimaryDevice!.Logical,
							new PipelineCache(),
							1,
							&pipelineInfo,
							null,
							&pipeline
						)) != Result.Success
					) {
						throw new PlatformException($"Could not create the graphics pipeline: {result}");
					}
					
					break;
				default:
					throw new NotImplementedException(Queue.Type.ToString());
			}
			
			Pipeline = pipeline;
		#endregion
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);

			unsafe {
				_platform.API.DestroyPipeline(_platform.PrimaryDevice.Logical, Pipeline, null);
				_platform.API.DestroyPipelineLayout(_platform.PrimaryDevice.Logical, PipelineLayout, null);
			}
			
			Queue.Dispose();
		}
	}
}
