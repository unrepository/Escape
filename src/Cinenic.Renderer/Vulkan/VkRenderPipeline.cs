using System.Diagnostics;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
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

		public unsafe VkRenderPipeline(VkPlatform platform, VkRenderQueue queue, IShaderPipeline shaderPipeline)
			: base(platform, queue, shaderPipeline)
		{
			//Debug.Assert(((VkRenderQueue) queue).Base.Handle != 0, "RenderQueue.Handle is 0. Did you forget to call Initialize()?");
			_platform = platform;

			((VkShaderProgram) shaderPipeline.Program.Get()).Build();

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
					X = Queue.Viewport.X,
					Y = Queue.Viewport.Y,
					Width = Queue.Viewport.Z > 0 ? Queue.Viewport.Z : 640,
					Height = Queue.Viewport.W > 0 ? Queue.Viewport.W : 480,
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
				
				bool hasDepthStencil = queue.Subpasses.Any(
					description => description.PDepthStencilAttachment is not null);

				var depthStencilState = new PipelineDepthStencilStateCreateInfo {
					SType = StructureType.PipelineDepthStencilStateCreateInfo,
					DepthTestEnable = hasDepthStencil,
					DepthWriteEnable = hasDepthStencil,
					DepthCompareOp = hasDepthStencil ? CompareOp.Less : CompareOp.Never
				};

				var pushConstantRange = new PushConstantRange {
					Offset = 0,
					Size = 64,
					StageFlags = ShaderStageFlags.All
				};
				
				var setLayouts = ((VkShaderProgram) shaderPipeline.Program.Get()).DescriptorSetLayouts.ToArray();

				fixed(DescriptorSetLayout* setLayoutsPtr = setLayouts) {
					var pipelineLayoutInfo = new PipelineLayoutCreateInfo {
						SType = StructureType.PipelineLayoutCreateInfo,
						SetLayoutCount = (uint) setLayouts.Length,
						PSetLayouts = setLayoutsPtr,
						PushConstantRangeCount = 1,
						PPushConstantRanges = &pushConstantRange
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
								StageCount = (uint) ((VkShaderProgram) Program.Get()).Stages.Count,
								Layout = PipelineLayout,
								RenderPass = ((VkRenderQueue) Queue).Base,
								Subpass = 0,
								BasePipelineHandle = default
							}
						};

						var programStages = ((VkShaderProgram) Program.Get()).Stages.ToArray();

						fixed(PipelineShaderStageCreateInfo* ptr = programStages) {
							pipelineInfo.PStages = ptr;

							pipelineInfo.PDynamicState = &dynamicStateInfo;
							pipelineInfo.PVertexInputState = &vertexInputInfo;
							pipelineInfo.PInputAssemblyState = &inputAssemblyInfo;
							pipelineInfo.PViewportState = &viewportInfo;
							pipelineInfo.PRasterizationState = &rasterizationInfo;
							pipelineInfo.PMultisampleState = &multisampleInfo;
							pipelineInfo.PColorBlendState = &colorBlendInfo;
							pipelineInfo.PDepthStencilState = &depthStencilState;

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

		public override bool Begin() {
			Debug.Assert(Queue is VkRenderQueue);
			Debug.Assert(Queue.RenderTarget is not null, "Queue.RenderTarget is null! Did you forget to assign it?");

			if(Queue.RenderTarget is VkWindow.WindowFramebuffer windowFramebuffer) {
				windowFramebuffer.Window.Base.DoUpdate();
				if(!windowFramebuffer.Window.Base.IsClosing) windowFramebuffer.Window.Base.DoEvents();
				if(windowFramebuffer.Window.Base.IsClosing) {
					Queue.RenderTarget.Dispose();
					Queue.RenderTarget = null;
					return false;
				}
				windowFramebuffer.Window.Base.MakeCurrent();
			}
			
			if(!Queue.Begin()) {
				RecreateFramebuffer(
					_platform,
					(VkFramebuffer) Queue.RenderTarget,
					out var newFramebuffer
				);
				
				RecreateQueue(
					_platform,
					(VkRenderQueue) Queue,
					out var newQueue
				);
				
				Queue = newQueue;
				Queue.RenderTarget = newFramebuffer;
			}

			var vkQueue = (VkRenderQueue) Queue;
			var vkRenderTarget = (VkFramebuffer) Queue.RenderTarget;
			
			_platform.API.CmdBindPipeline(
				vkQueue.CommandBuffer,
				Queue.Type switch {
					RenderQueue.Family.Graphics => PipelineBindPoint.Graphics,
					RenderQueue.Family.Compute => PipelineBindPoint.Compute,
					_ => throw new NotImplementedException()
				},
				Pipeline
			);
			
			Program.Get().Program.Bind(this);
			
			var viewport = new Viewport {
				X = Queue.Viewport.X,
				Y = Queue.Viewport.Y,
				Width = Queue.Viewport.Z > 0 ? Queue.Viewport.Z : vkRenderTarget.Size.X,
				Height = Queue.Viewport.W > 0 ? Queue.Viewport.W : vkRenderTarget.Size.Y,
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

			return true;
		}
		
		public override bool End() {
			Debug.Assert(Queue is VkRenderQueue);
			Debug.Assert(Queue.RenderTarget is not null, "Queue.RenderTarget is null! Did you forget to assign it?");
			
			if(!Queue.End()) {
				RecreateFramebuffer(
					_platform,
					(VkFramebuffer) Queue.RenderTarget,
					out var newFramebuffer
				);
				
				RecreateQueue(
					_platform,
					(VkRenderQueue) Queue,
					out var newQueue
				);
				
				Queue = newQueue;
				Queue.RenderTarget = newFramebuffer;
			}

			return true;
		}
		
		public static void RecreateFramebuffer(VkPlatform platform, in VkFramebuffer oldFramebuffer, out VkFramebuffer newFramebuffer) {
			platform.API.DeviceWaitIdle(platform.PrimaryDevice!.Logical);
			oldFramebuffer.Dispose();

			if(oldFramebuffer is VkWindow.WindowFramebuffer windowFramebuffer) {
				newFramebuffer = new VkWindow.WindowFramebuffer(
					platform,
					(VkRenderQueue) windowFramebuffer.Queue,
					windowFramebuffer.Window,
					windowFramebuffer.Size
				);
			} else {
				newFramebuffer = new VkFramebuffer(platform, (VkRenderQueue) oldFramebuffer.Queue, oldFramebuffer.Size);
			}
		}

		public static void RecreateQueue(VkPlatform platform, in VkRenderQueue oldQueue, out VkRenderQueue newQueue) {
			platform.API.DeviceWaitIdle(platform.PrimaryDevice!.Logical);
			oldQueue.Dispose();
			
			newQueue = new VkRenderQueue(platform, oldQueue.Type, oldQueue.ColorFormat) {
				Attachments = oldQueue.Attachments,
				Subpasses = oldQueue.Subpasses,
				SubpassDependencies = oldQueue.SubpassDependencies,
				Viewport = oldQueue.Viewport,
				Scissor = oldQueue.Scissor
			};
			newQueue.Initialize();
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
