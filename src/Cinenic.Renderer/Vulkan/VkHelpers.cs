using System.Diagnostics;
using System.Runtime.CompilerServices;
using NLog;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Cinenic.Renderer.Vulkan {
	
	public static class VkHelpers {

		private static Logger _logger = LogManager.GetCurrentClassLogger();
		
		public static bool VkCheck(
			Result result, string errorMessage,
			bool fatal = true,
			[CallerFilePath] string file = "",
			[CallerLineNumber] int line = 0,
			[CallerMemberName] string member = ""
		) {
			if(result != Result.Success) {
				string msg = $"VkCheck failed @ {file}:{line}/{member} (result == {result}): {errorMessage}";
				
				if(!fatal) {
					_logger.Error(msg);
					return false;
				}
				
				throw new PlatformException(msg);
			}

			return true;
		}
		
		public static uint FindMemoryType(PhysicalDeviceMemoryProperties properties, uint typeFilter, MemoryPropertyFlags flags) {
			for(uint i = 0; i < properties.MemoryTypeCount; i++) {
				if(
					(typeFilter & (1 << (int) i)) != 0
					&& (properties.MemoryTypes[(int) i].PropertyFlags & flags) == flags
				) {
					return i;
				}
			}

			_logger.Warn("Was unable to find the desired memory type");
			return 0;
		}

		public unsafe static void AllocateBuffer(
			VkPlatform platform,
			uint size,
			ref Buffer buffer, ref DeviceMemory bufferMemory,
			BufferUsageFlags usageFlags = BufferUsageFlags.VertexBufferBit | BufferUsageFlags.StorageBufferBit,
			MemoryPropertyFlags memoryFlags = MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
		) {
			var device = platform.PrimaryDevice.Logical;
			
			// create buffer
			var bufferInfo = new BufferCreateInfo {
				SType = StructureType.BufferCreateInfo,
				Size = size,
				Usage = usageFlags,
				SharingMode = SharingMode.Exclusive
			};

			platform.API.CreateBuffer(
				device,
				bufferInfo,
				null,
				out buffer
			);
			
			// allocate memory
			platform.API.GetPhysicalDeviceMemoryProperties(platform.PrimaryDevice.Physical, out var memoryProperties);
			platform.API.GetBufferMemoryRequirements(device, buffer, out var memoryRequirements);
			
			var allocateInfo = new MemoryAllocateInfo {
				SType = StructureType.MemoryAllocateInfo,
				AllocationSize = memoryRequirements.Size,
				MemoryTypeIndex = FindMemoryType(
					memoryProperties,
					memoryRequirements.MemoryTypeBits,
					memoryFlags
				)
			};

			platform.API.AllocateMemory(device, allocateInfo, null, out bufferMemory);
			platform.API.BindBufferMemory(device, buffer, bufferMemory, 0);
		}

		public unsafe static void TransitionImageLayout(
			VkPlatform platform,
			CommandBuffer commandBuffer,
			Image image,
			ImageLayout oldLayout,
			ImageLayout newLayout
		) {
			var barrier = new ImageMemoryBarrier {
				SType = StructureType.ImageMemoryBarrier,
				OldLayout = oldLayout,
				NewLayout = newLayout,
				SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
				DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
				Image = image,
				SubresourceRange = new ImageSubresourceRange {
					AspectMask = ImageAspectFlags.ColorBit,
					BaseMipLevel = 0,
					LevelCount = 1,
					BaseArrayLayer = 0,
					LayerCount = 1
				}
			};

			var srcStageMask = PipelineStageFlags.None;
			var dstStageMask = PipelineStageFlags.None;

			if(oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal) {
				barrier.SrcAccessMask = AccessFlags.None;
				barrier.DstAccessMask = AccessFlags.TransferWriteBit;

				srcStageMask = PipelineStageFlags.TopOfPipeBit;
				dstStageMask = PipelineStageFlags.TransferBit;
			} else if(oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal) {
				barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
				barrier.DstAccessMask = AccessFlags.ShaderReadBit;

				srcStageMask = PipelineStageFlags.TransferBit;
				dstStageMask = PipelineStageFlags.FragmentShaderBit;
			}
				
			platform.API.CmdPipelineBarrier(
				commandBuffer,
				srcStageMask,
				dstStageMask,
				0,
				0,
				null,
				0,
				null,
				1,
				barrier
			);
		}

		public unsafe static (DescriptorSet Set, DescriptorSetLayout Layout, Dictionary<uint, DescriptorSetLayoutBinding> Bindings) CreateDescriptorSet(
			VkPlatform platform,
			VkShaderProgram? program,
			uint[] bindings,
			DescriptorType type,
			ShaderStageFlags stageFlags,
			DescriptorBindingFlags bindingFlags = DescriptorBindingFlags.None
		) {
			var descriptorBindings = new Dictionary<uint, DescriptorSetLayoutBinding>();
			var device = platform.PrimaryDevice.Logical;
			
			// create descriptor pool
			var descriptorPoolSize = new DescriptorPoolSize {
				Type = type,
				DescriptorCount = (uint) bindings.Length
			};

			var descriptorPoolInfo = new DescriptorPoolCreateInfo {
				SType = StructureType.DescriptorPoolCreateInfo,
				PoolSizeCount = 1,
				PPoolSizes = &descriptorPoolSize,
				MaxSets = (uint) bindings.Length
			};

			platform.API.CreateDescriptorPool(device, descriptorPoolInfo, null, out var descriptorPool);

			var descriptorLayoutBindings = new DescriptorSetLayoutBinding[bindings.Length];
			var descriptorBindingFlags = new DescriptorBindingFlags[bindings.Length];
			
			for(uint i = 0; i < bindings.Length; i++) {
				var descriptorLayoutBinding = new DescriptorSetLayoutBinding {
					Binding = bindings[i],
					DescriptorType = type,
					DescriptorCount = 1,
					StageFlags = stageFlags
				};

				descriptorLayoutBindings[i] = descriptorLayoutBinding;
				descriptorBindingFlags[i] = bindingFlags;
				
				descriptorBindings[bindings[i]] = descriptorLayoutBinding;
			}
			
			DescriptorSetLayout descriptorLayout;
			DescriptorSet descriptorSet;
			
			fixed(
				void* descriptorLayoutBindingsPtr = descriptorLayoutBindings,
				descriptorBindingFlagsPtr = descriptorBindingFlags
			) {
				var descriptorSetLayoutBindingFlagsInfo = new DescriptorSetLayoutBindingFlagsCreateInfo {
					SType = StructureType.DescriptorSetLayoutBindingFlagsCreateInfo,
					BindingCount = (uint) bindings.Length,
					PBindingFlags = (DescriptorBindingFlags*) descriptorBindingFlagsPtr
				};
				
				var descriptorLayoutInfo = new DescriptorSetLayoutCreateInfo {
					SType = StructureType.DescriptorSetLayoutCreateInfo,
					BindingCount = (uint) bindings.Length,
					PBindings = (DescriptorSetLayoutBinding*) descriptorLayoutBindingsPtr,
					PNext = &descriptorSetLayoutBindingFlagsInfo
				};

				platform.API.CreateDescriptorSetLayout(device, descriptorLayoutInfo, null, out descriptorLayout);
				program?.DescriptorSetLayouts.Add(descriptorLayout);
				
				// create descriptor set
				var descriptorAllocateInfo = new DescriptorSetAllocateInfo {
					SType = StructureType.DescriptorSetAllocateInfo,
					DescriptorPool = descriptorPool,
					DescriptorSetCount = 1,
					PSetLayouts = &descriptorLayout
				};

				platform.API.AllocateDescriptorSets(device, descriptorAllocateInfo, out descriptorSet);
				program?.DescriptorSets.Add(descriptorSet);
			}

			return (descriptorSet, descriptorLayout, descriptorBindings);
		}
	}
}
