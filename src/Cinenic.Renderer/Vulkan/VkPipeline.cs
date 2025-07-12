using NLog;
using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkPipeline : IDisposable {

		public Viewport Viewport { get; private set; }
		public Rect2D Scissor { get; private set; }
		
		internal readonly PipelineDynamicStateCreateInfo DynamicStateInfo;
		internal readonly PipelineVertexInputStateCreateInfo VertexInputInfo;
		internal readonly PipelineInputAssemblyStateCreateInfo InputAssemblyInfo;
		internal readonly PipelineViewportStateCreateInfo ViewportInfo;
		internal readonly PipelineRasterizationStateCreateInfo RasterizationInfo;
		internal readonly PipelineMultisampleStateCreateInfo MultisampleInfo;
		internal readonly PipelineColorBlendStateCreateInfo ColorBlendInfo;

		internal readonly PipelineLayout PipelineLayout;

		internal RenderPass? MainRenderPass { get; private set; }
		internal Pipeline? GraphicsPipeline { get; private set; }

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private static readonly DynamicState[] _dynamicStates = [
			DynamicState.Viewport,
			DynamicState.Scissor,
		];
		
		private readonly VkPlatform _platform;
		
		public unsafe VkPipeline(VkPlatform platform) {
			_platform = platform;

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
			
			_logger.Debug("Pipeline setup: Create layout");
		}

		public unsafe void InitializeGraphics(VkWindow window, VkShaderProgram shaderProgram) {
			Viewport = new Viewport {
				X = 0,
				Y = 0,
				Width = window.SwapchainExtent.Width,
				Height = window.SwapchainExtent.Height,
				MinDepth = 0.0f,
				MaxDepth = 1.0f
			};

			Scissor = new Rect2D {
				Offset = { X = 0, Y = 0 },
				Extent = window.SwapchainExtent
			};
			
			MainRenderPass = new RenderPass(_platform, this);
			MainRenderPass.CreateAttachment(window.SwapchainFormat);
			MainRenderPass.CreateSubpass(0, ImageLayout.ColorAttachmentOptimal, new SubpassDescription {
				PipelineBindPoint = PipelineBindPoint.Graphics,
				ColorAttachmentCount = 1
			});
			MainRenderPass.Initialize();

			var pipelineInfo = new GraphicsPipelineCreateInfo {
				SType = StructureType.GraphicsPipelineCreateInfo,
				StageCount = (uint) shaderProgram.Stages.Count,
				Layout = PipelineLayout,
				RenderPass = MainRenderPass.Base,
				Subpass = 0,
				BasePipelineHandle = new Pipeline(),
				BasePipelineIndex = -1
			};

			fixed(PipelineShaderStageCreateInfo* ptr = &shaderProgram.Stages.ToArray()[0]) {
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

			Result result;
			var pipeline = new Pipeline();
			
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

			GraphicsPipeline = pipeline;
			_logger.Debug("Pipeline setup: Create graphics pipeline");
		}

		public void Dispose() {
			GC.SuppressFinalize(this);

			unsafe {
				if(GraphicsPipeline.HasValue) {
					_platform.API.DestroyPipeline(_platform.PrimaryDevice.Logical, GraphicsPipeline.Value, null);
				}
				
				_platform.API.DestroyPipelineLayout(_platform.PrimaryDevice.Logical, PipelineLayout, null);
			}
			
			MainRenderPass?.Dispose();
		}

		public class RenderPass : IDisposable {

			public Silk.NET.Vulkan.RenderPass Base;
			
			internal readonly List<AttachmentDescription> Attachments = [];
			internal readonly List<SubpassDescription> Subpasses = [];

			private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
			
			private readonly VkPlatform _platform;
			private readonly VkPipeline _pipeline;
			
			public RenderPass(VkPlatform platform, VkPipeline pipeline) {
				_platform = platform;
				_pipeline = pipeline;
			}

			public unsafe void Initialize() {
				var attachments = Attachments.ToArray();
				var subpasses = Subpasses.ToArray();

				fixed(AttachmentDescription* attachmentsPtr = &attachments[0]) {
					fixed(SubpassDescription* subpassesPtr = &subpasses[0]) {
						var renderPassInfo = new RenderPassCreateInfo {
							SType = StructureType.RenderPassCreateInfo,
							AttachmentCount = (uint) attachments.Length,
							PAttachments = attachmentsPtr,
							SubpassCount = (uint) subpasses.Length,
							PSubpasses = subpassesPtr
						};

						Result result;
						if(
							(result = _platform.API.CreateRenderPass(_platform.PrimaryDevice!.Logical, &renderPassInfo, null, out var pass))
							!= Result.Success
						) {
							throw new PlatformException($"Could not create render pass: {result}");
						}

						Base = pass;
					}
				}
				
				_logger.Debug("Initialized");
			}

			public void CreateAttachment(Format format) {
				var attachment = new AttachmentDescription {
					Format = format,
					Samples = SampleCountFlags.Count1Bit,
					LoadOp = AttachmentLoadOp.Clear,
					StoreOp = AttachmentStoreOp.Store,
					StencilLoadOp = AttachmentLoadOp.DontCare,
					StencilStoreOp = AttachmentStoreOp.DontCare,
					InitialLayout = ImageLayout.Undefined,
					FinalLayout = ImageLayout.PresentSrcKhr
				};
				
				Attachments.Add(attachment);
				
				_logger.Debug("Created attachment for {RenderPass}", this);
			}

			public unsafe void CreateSubpass(uint attachmentIndex, ImageLayout layout, SubpassDescription description) {
				var attachmentReference = new AttachmentReference {
					Attachment = attachmentIndex,
					Layout = layout
				};

				description.PColorAttachments = &attachmentReference;
				description.PInputAttachments = &attachmentReference;
				
				Subpasses.Add(description);
				
				_logger.Debug("Created subpass for {RenderPass}, attachmentIndex={AttachmentIndex}, layout={Layout}, bindPoint={BindPoint}",
					this, attachmentIndex, layout, description.PipelineBindPoint);
			}
			
			public void Dispose() {
				GC.SuppressFinalize(this);

				unsafe {
					_platform.API.DestroyRenderPass(_platform.PrimaryDevice.Logical, Base, null);
				}
			}
		}
	}
}
