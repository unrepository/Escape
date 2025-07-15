using System.Diagnostics;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.Core;
using Silk.NET.Vulkan;

using static Cinenic.Renderer.Vulkan.VkHelpers;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkRenderPipeline : RenderPipeline {

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

				VkCheck(
					_platform.API.CreatePipelineLayout(
						_platform.PrimaryDevice!.Logical,
						pipelineLayoutInfo,
						null,
						out PipelineLayout
					),
					"Could not create the pipeline layout"
				);
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
								RenderPass = ((VkRenderQueue) Queue).RenderPass,
								Subpass = 0,
								BasePipelineHandle = default,
							}
						};

						var programStages = ((VkShaderProgram) Program).Stages.ToArray();

						fixed(PipelineShaderStageCreateInfo* ptr = programStages) {
							pipelineInfo.PStages = ptr;

							pipelineInfo.PDynamicState = &dynamicStateInfo;
							pipelineInfo.PVertexInputState = &vertexInputInfo;
							pipelineInfo.PInputAssemblyState = &inputAssemblyInfo;
							pipelineInfo.PViewportState = &viewportInfo;
							pipelineInfo.PRasterizationState = &rasterizationInfo;
							pipelineInfo.PMultisampleState = &multisampleInfo;
							pipelineInfo.PColorBlendState = &colorBlendInfo;

							VkCheck(
								_platform.API.CreateGraphicsPipelines(
									_platform.PrimaryDevice!.Logical,
									default,
									1,
									pipelineInfo,
									null,
									out pipeline
								),
								"Could not create the graphics pipeline"
							);
						}
						
						break;
					default:
						throw new NotImplementedException(Queue.Type.ToString());
				}

				Pipeline = pipeline;
			}
		#endregion
		}

		public override void Begin(ref Framebuffer renderTarget) {
			var vkRenderTarget = (VkFramebuffer) renderTarget;
			var vkQueue = (VkRenderQueue) Queue;
			
			if(!Queue.Begin(renderTarget)) {
				RecreateFramebuffer(ref renderTarget);
				RecreateQueue(ref vkQueue);

				vkRenderTarget = (VkFramebuffer) renderTarget;
				Queue = vkQueue;
			}
			
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
				Width = Queue.Viewport.Z > 0 ? Queue.Viewport.Z : vkRenderTarget.SwapchainExtent.Width,
				Height = Queue.Viewport.W > 0 ? Queue.Viewport.W : vkRenderTarget.SwapchainExtent.Height,
				MinDepth = 0,
				MaxDepth = 1
			};
			
			var scissor = new Rect2D {
				Offset = {
					X = Queue.Scissor.X,
					Y = Queue.Scissor.Y
				},
				Extent = {
					Width = (uint) (Queue.Scissor.Z > 0 ? Queue.Scissor.Z : viewport.Width),
					Height = (uint) (Queue.Scissor.W > 0 ? Queue.Scissor.W : viewport.Height)
				}
			};
			
			unsafe {
				_platform.API.CmdSetViewport(vkQueue.CommandBuffer, 0, 1, &viewport);
				_platform.API.CmdSetScissor(vkQueue.CommandBuffer, 0, 1, &scissor);
			}
		}
		
		public override void End(ref Framebuffer renderTarget) {
			var vkQueue = (VkRenderQueue) Queue;
			
			if(!Queue.End(renderTarget)) {
				RecreateFramebuffer(ref renderTarget);
				RecreateQueue(ref vkQueue);
				Queue = vkQueue;
			}
		}

		public void RecreateQueue(ref VkRenderQueue vkQueue) {
			//_logger.Trace("Recreating render queue");

			_platform.API.DeviceWaitIdle(_platform.PrimaryDevice!.Logical);
			
			Queue.Dispose();
			
			var newQueue = new VkRenderQueue(_platform, vkQueue.Type, vkQueue.ColorFormat) {
				Attachments = vkQueue.Attachments,
				Subpasses = vkQueue.Subpasses,
				SubpassDependencies = vkQueue.SubpassDependencies,
				Viewport = vkQueue.Viewport,
				Scissor = vkQueue.Scissor
			};
			newQueue.Initialize();
			
			Queue = newQueue;
			vkQueue = newQueue;
		}
		
		public void RecreateFramebuffer(ref Framebuffer renderTarget) {
			Debug.Assert(renderTarget is VkFramebuffer);
			//_logger.Trace("Recreating framebuffer");

			_platform.API.DeviceWaitIdle(_platform.PrimaryDevice!.Logical);
			renderTarget.Dispose();
			
			VkFramebuffer newFramebuffer;

			if(renderTarget is VkWindow.WindowFramebuffer windowFramebuffer) {
				newFramebuffer = new VkWindow.WindowFramebuffer(_platform, windowFramebuffer.Window, windowFramebuffer.Size);
			} else {
				newFramebuffer = new VkFramebuffer(_platform, renderTarget.Size);
			}
			
			renderTarget = newFramebuffer;
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);

			_platform.API.DeviceWaitIdle(_platform.PrimaryDevice.Logical);
			
			unsafe {
				_platform.API.DestroyPipeline(_platform.PrimaryDevice.Logical, Pipeline, null);
				_platform.API.DestroyPipelineLayout(_platform.PrimaryDevice.Logical, PipelineLayout, null);
			}
			
			Queue.Dispose();
		}
	}
}
