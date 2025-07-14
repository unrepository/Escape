using System.Diagnostics;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkRenderPipeline : RenderPipeline {
		
		// internal readonly PipelineDynamicStateCreateInfo DynamicStateInfo;
		// internal readonly PipelineVertexInputStateCreateInfo VertexInputInfo;
		// internal readonly PipelineInputAssemblyStateCreateInfo InputAssemblyInfo;
		// internal readonly PipelineViewportStateCreateInfo ViewportInfo;
		// internal readonly PipelineRasterizationStateCreateInfo RasterizationInfo;
		// internal readonly PipelineMultisampleStateCreateInfo MultisampleInfo;
		// internal readonly PipelineColorBlendStateCreateInfo ColorBlendInfo;

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
			Debug.Assert(queue is VkRenderQueue);
			//Debug.Assert(((VkRenderQueue) queue).Base.Handle != 0, "RenderQueue.Handle is 0. Did you forget to call Initialize()?");
			_platform = platform;

			program.Build();

		#region Pipeline layout
			_logger.Debug("Pipeline setup: Create layout");

			fixed(DynamicState* dynamicStatesPtr = &_dynamicStates[0]) {
				var dynamicStateInfo = new PipelineDynamicStateCreateInfo {
					SType = StructureType.PipelineDynamicStateCreateInfo,
					DynamicStateCount = (uint) _dynamicStates.Length,
					PDynamicStates = dynamicStatesPtr
				};

				var vertexInputInfo = new PipelineVertexInputStateCreateInfo {
					SType = StructureType.PipelineVertexInputStateCreateInfo,
					VertexBindingDescriptionCount = 0,
					PVertexBindingDescriptions = null,
					VertexAttributeDescriptionCount = 0,
					PVertexAttributeDescriptions = null
				};

				var inputAssemblyInfo = new PipelineInputAssemblyStateCreateInfo {
					SType = StructureType.PipelineInputAssemblyStateCreateInfo,
					Topology = PrimitiveTopology.TriangleList,
					PrimitiveRestartEnable = false
				};

				var viewport = new Viewport {
					X = queue.Viewport.X,
					Y = queue.Viewport.Y,
					Width = queue.Viewport.Z,
					Height = queue.Viewport.W,
					MinDepth = 0,
					MaxDepth = 1
				};

				var scissor = new Rect2D {
					Offset = { X = queue.Viewport.X, Y = queue.Viewport.Y },
					Extent = { Width = (uint) queue.Viewport.Z, Height = (uint) queue.Viewport.W }
				};

				var viewportInfo = new PipelineViewportStateCreateInfo {
					SType = StructureType.PipelineViewportStateCreateInfo,
					ViewportCount = 1,
					PViewports = &viewport,
					ScissorCount = 1,
					PScissors = &scissor
				};

				var rasterizationInfo = new PipelineRasterizationStateCreateInfo {
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

				var multisampleInfo = new PipelineMultisampleStateCreateInfo {
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

				var colorBlendInfo = new PipelineColorBlendStateCreateInfo {
					SType = StructureType.PipelineColorBlendStateCreateInfo,
					LogicOpEnable = false,
					LogicOp = LogicOp.Copy,
					AttachmentCount = 1,
					PAttachments = &colorBlendAttachment
				};

				colorBlendInfo.BlendConstants[0] = 0;
				colorBlendInfo.BlendConstants[1] = 0;
				colorBlendInfo.BlendConstants[2] = 0;
				colorBlendInfo.BlendConstants[3] = 0;

				var pipelineLayoutInfo = new PipelineLayoutCreateInfo {
					SType = StructureType.PipelineLayoutCreateInfo,
					SetLayoutCount = 0,
					PSetLayouts = null,
					PushConstantRangeCount = 0,
					PPushConstantRanges = null
				};

				Result result;

				if(
					(result = _platform.API.CreatePipelineLayout(
						_platform.PrimaryDevice!.Logical,
						pipelineLayoutInfo,
						null,
						out PipelineLayout)
					)
					!= Result.Success
				) {
					throw new PlatformException($"Could not create pipeline layout: {result}");
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
								BasePipelineHandle = default,
							}
						};

						var programStages = ((VkShaderProgram) Program).Stages.ToArray();

						fixed(PipelineShaderStageCreateInfo* ptr = &programStages[0]) {
							pipelineInfo.PStages = ptr;

							pipelineInfo.PDynamicState = &dynamicStateInfo;
							pipelineInfo.PVertexInputState = &vertexInputInfo;
							pipelineInfo.PInputAssemblyState = &inputAssemblyInfo;
							pipelineInfo.PViewportState = &viewportInfo;
							pipelineInfo.PRasterizationState = &rasterizationInfo;
							pipelineInfo.PMultisampleState = &multisampleInfo;
							pipelineInfo.PColorBlendState = &colorBlendInfo;

							result = _platform.API.CreateGraphicsPipelines(
								_platform.PrimaryDevice!.Logical,
								default,
								1,
								pipelineInfo,
								null,
								out pipeline
							);
						}

						if(
							(result) != Result.Success
						) {
							throw new PlatformException($"Could not create the graphics pipeline: {result}");
						}

						break;
					default:
						throw new NotImplementedException(Queue.Type.ToString());
				}

				Pipeline = pipeline;
			}
		#endregion
		}

		public override void Begin(Framebuffer renderTarget) {
			Queue.Begin(renderTarget);

			var vkQueue = (VkRenderQueue) Queue;
			
			_platform.API.CmdBindPipeline(
				vkQueue.CommandBuffer,
				Queue.Type switch {
					RenderQueue.Family.Graphics => PipelineBindPoint.Graphics,
					RenderQueue.Family.Compute => PipelineBindPoint.Compute,
					_ => throw new NotImplementedException()
				},
				Pipeline
			);
			
			var viewport = new Viewport {
				X = Queue.Viewport.X,
				Y = Queue.Viewport.Y,
				Width = Queue.Viewport.Z,
				Height = Queue.Viewport.W,
				MinDepth = 0,
				MaxDepth = 1
			};
			
			var scissor = new Rect2D {
				Offset = { X = Queue.Viewport.X, Y = Queue.Viewport.Y },
				Extent = { Width = (uint) Queue.Viewport.Z, Height = (uint) Queue.Viewport.W }
			};

			unsafe {
				_platform.API.CmdSetViewport(vkQueue.CommandBuffer, 0, 1, &viewport);
				_platform.API.CmdSetScissor(vkQueue.CommandBuffer, 0, 1, &scissor);
			}
		}
		
		public override void End(Framebuffer renderTarget) {
			Queue.End(renderTarget);
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);

			unsafe {
				_platform.API.DeviceWaitIdle(_platform.PrimaryDevice.Logical);
				
				_platform.API.DestroyPipeline(_platform.PrimaryDevice.Logical, Pipeline, null);
				_platform.API.DestroyPipelineLayout(_platform.PrimaryDevice.Logical, PipelineLayout, null);
			}
			
			Queue.Dispose();
		}
	}
}
