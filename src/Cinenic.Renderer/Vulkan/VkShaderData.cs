using System.Diagnostics;
using System.Runtime.InteropServices;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Cinenic.Renderer.Vulkan {
	
	public unsafe class VkShaderData<T> : IShaderData<T> {
		
		public ulong Handle { get; private set; }
		public uint Binding { get; }
		
		public T? Data {
			get => _data;
			set => _data = value;
		}
		
		public uint Size { get; set; }

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly VkPlatform _platform;
		private readonly VkShaderProgram _program;
		
		private T? _data;
		private void* _bufferDataPtr = null;
		private uint _lastSize;

		private Buffer _buffer;
		private DeviceMemory _bufferMemory;

		public VkShaderData(
			VkPlatform platform,
			ShaderProgram program,
			uint binding,
			T? data,
			uint size
		) {
			_platform = platform;
			_program = (VkShaderProgram) program;

			Binding = binding;
			Data = data;
			Size = _lastSize = size;

			if(size > 0) {
				Create(
					_platform,
					_program,
					Binding,
					Size,
					ref _buffer,
					ref _bufferMemory
				);
			}
		}
		
		public void Push() {
			Debug.Assert(_platform.PrimaryDevice is not null);
			
			// recreate if data size changed
			if(Size != _lastSize) {
				Debug.Assert(Size > 0);
				
				Dispose();
				Create(
					_platform,
					_program,
					Binding,
					Size,
					ref _buffer,
					ref _bufferMemory
				);
			}
			
			if(Data is null) {
				return;
			}

			fixed(void* dataPtr = &_data) {
				if(_bufferDataPtr is null) {
					_platform.API.MapMemory(
						_platform.PrimaryDevice.Logical,
						_bufferMemory,
						0,
						Size,
						0,
						ref _bufferDataPtr
					);
				}

				Debug.Assert(_bufferDataPtr is not null);

				System.Buffer.MemoryCopy(dataPtr, _bufferDataPtr, Size, Size);
				_lastSize = Size;
			}
		}
		
		public void Read() {
			throw new NotImplementedException();
		}
		
		public void Dispose() {
			GC.SuppressFinalize(this);

			Handle = 0;
			_platform.API.UnmapMemory(_platform.PrimaryDevice.Logical, _bufferMemory);
			_platform.API.DestroyBuffer(_platform.PrimaryDevice.Logical, _buffer, null);
			_platform.API.FreeMemory(_platform.PrimaryDevice.Logical, _bufferMemory, null);
		}
		
		internal static void Create(
			VkPlatform platform,
			VkShaderProgram program,
			uint binding,
			uint size,
			ref Buffer buffer,
			ref DeviceMemory bufferMemory
		) {
			uint FindMemoryType(PhysicalDeviceMemoryProperties properties, uint typeFilter, MemoryPropertyFlags flags) {
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
			
			Debug.Assert(platform.PrimaryDevice is not null);
			var device = platform.PrimaryDevice.Logical;
			
			Debug.Assert(bufferMemory.Handle == 0);
			
			// create buffer
			var bufferInfo = new BufferCreateInfo {
				SType = StructureType.BufferCreateInfo,
				Size = size,
				Usage = BufferUsageFlags.VertexBufferBit | BufferUsageFlags.StorageBufferBit,
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
					MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
				)
			};

			platform.API.AllocateMemory(device, allocateInfo, null, out bufferMemory);
			platform.API.BindBufferMemory(device, buffer, bufferMemory, 0);
			
			// set up layout
			var descriptorLayoutBinding = new DescriptorSetLayoutBinding {
				Binding = binding,
				DescriptorType = DescriptorType.StorageBuffer,
				DescriptorCount = 1,
				StageFlags = ShaderStageFlags.All
			};

			var descriptorLayoutInfo = new DescriptorSetLayoutCreateInfo {
				SType = StructureType.DescriptorSetLayoutCreateInfo,
				BindingCount = 1,
				PBindings = &descriptorLayoutBinding
			};

			platform.API.CreateDescriptorSetLayout(device, descriptorLayoutInfo, null, out var descriptorLayout);
			program.DescriptorSetLayouts.Add(descriptorLayout);
			
			// create descriptor pool
			var descriptorPoolSize = new DescriptorPoolSize {
				Type = DescriptorType.StorageBuffer,
				DescriptorCount = 1
			};

			var descriptorPoolInfo = new DescriptorPoolCreateInfo {
				SType = StructureType.DescriptorPoolCreateInfo,
				PoolSizeCount = 1,
				PPoolSizes = &descriptorPoolSize,
				MaxSets = 1
			};

			platform.API.CreateDescriptorPool(device, descriptorPoolInfo, null, out var descriptorPool);
			
			// create descriptor set
			var descriptorAllocateInfo = new DescriptorSetAllocateInfo {
				SType = StructureType.DescriptorSetAllocateInfo,
				DescriptorPool = descriptorPool,
				DescriptorSetCount = 1,
				PSetLayouts = &descriptorLayout
			};

			platform.API.AllocateDescriptorSets(device, descriptorAllocateInfo, out var descriptorSet);

			// update descriptor set
			var descriptorBufferInfo = new DescriptorBufferInfo {
				Buffer = buffer,
				Offset = 0,
				Range = size
			};

			var writeDescriptorSet = new WriteDescriptorSet {
				SType = StructureType.WriteDescriptorSet,
				DstSet = descriptorSet,
				DstBinding = binding,
				DstArrayElement = 0, // TODO what is this
				DescriptorType = descriptorLayoutBinding.DescriptorType,
				DescriptorCount = 1,
				PBufferInfo = &descriptorBufferInfo
			};
			
			platform.API.UpdateDescriptorSets(device, 1, writeDescriptorSet, 0, null);
			program.DescriptorSets.Add(descriptorSet);
		}
	}
}
