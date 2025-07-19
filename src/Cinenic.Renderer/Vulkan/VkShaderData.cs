using System.Diagnostics;
using System.Runtime.InteropServices;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

using static Cinenic.Renderer.Vulkan.VkHelpers;

namespace Cinenic.Renderer.Vulkan {
	
	public unsafe class VkShaderData<T> : IShaderData<T> {
		
		public ulong Handle { get; private set; }
		public uint Binding { get; }
		
		public T? Data {
			get => _data;
			set => _data = value;
		}

		public uint Size {
			get;
			set {
				if(_bufferSize == 0) {
					field = value;
					return;
				}
				
				Debug.Assert(value > 0);
				
				if(value > _bufferSize) {
					value = value.CeilIncrement(1024);
				
					_logger.Trace("Buffer size changed ({OldSize} -> {NewSize}); reallocating", _bufferSize, value);

					if(_bufferDataPtr is not null) {
						_platform.API.UnmapMemory(_platform.PrimaryDevice.Logical, _bufferMemory);
					}
				
					_platform.API.FreeMemory(_platform.PrimaryDevice.Logical, _bufferMemory, null);
					_platform.API.DestroyBuffer(_platform.PrimaryDevice.Logical, _buffer, null);

					_bufferDataPtr = AllocateMemory(
						_platform,
						value,
						ref _buffer,
						ref _bufferMemory
					);
				
					UpdateDescriptorSet(
						_platform,
						_descriptorSet,
						Binding,
						value,
						_buffer
					);

					_bufferSize = value;
				}

				field = value;
			}
		}

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private readonly VkPlatform _platform;
		private readonly VkShaderProgram _program;

		private readonly DescriptorSet _descriptorSet;
		
		private T? _data;
		private void* _bufferDataPtr;
		private uint _bufferSize;

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

			if(size == 0) size = 1024 * 1024; // 1 MiB
			
			Binding = binding;
			Data = data;
			Size = size;
			
			_bufferDataPtr = AllocateMemory(
				_platform,
				Size,
				ref _buffer,
				ref _bufferMemory
			);

			CreateDescriptorSet(
				_platform,
				_program,
				binding,
				out _descriptorSet
			);
			
			UpdateDescriptorSet(
				_platform,
				_descriptorSet,
				binding,
				Size,
				_buffer
			);
			
			_bufferSize = Size;
			
			/*if(Size > 0) {
				// _set = Create(
				// 	_platform,
				// 	_program,
				// 	Binding,
				// 	Size,
				// 	ref _buffer,
				// 	ref _bufferMemory
				// );
				
				_bufferSize = Size;
			} else {
				_logger.Warn("size == 0; will not allocate memory and descriptors which might lead to unknown errors!");
			}*/
		}
		
		public void Push() {
			Debug.Assert(_platform.PrimaryDevice is not null);
			Debug.Assert(_bufferDataPtr is not null);
			
			if(Data is null) {
				return;
			}

			fixed(void* dataPtr = &_data) {
				Debug.Assert(_bufferDataPtr is not null);
				System.Buffer.MemoryCopy(dataPtr, _bufferDataPtr, Size, Size);
			}
		}
		
		public void Read() {
			throw new NotImplementedException();
		}
		
		public void Write(uint offset, T data, uint? size = null) {
			size ??= (uint) sizeof(T);

			void* dst = (byte*) _bufferDataPtr + offset;
			System.Buffer.MemoryCopy(&data, dst, size.Value, size.Value);
		}
		
		public void Dispose() {
			GC.SuppressFinalize(this);

			Handle = 0;
			if(_bufferDataPtr is not null) _platform.API.UnmapMemory(_platform.PrimaryDevice.Logical, _bufferMemory);
			_platform.API.FreeMemory(_platform.PrimaryDevice.Logical, _bufferMemory, null);
			_platform.API.DestroyBuffer(_platform.PrimaryDevice.Logical, _buffer, null);

			_bufferDataPtr = null;
			_bufferMemory = default;
			_buffer = default;
		}

		internal static void* AllocateMemory(
			VkPlatform platform,
			uint size,
			ref Buffer buffer,
			ref DeviceMemory bufferMemory
		) {
			AllocateBuffer(platform, size, ref buffer, ref bufferMemory);

			void* data = null;
			
			platform.API.MapMemory(
				platform.PrimaryDevice.Logical,
				bufferMemory,
				0,
				size,
				0,
				ref data
			);

			return data;
		}

		internal static void CreateDescriptorSet(
			VkPlatform platform,
			VkShaderProgram program,
			uint binding,
			out DescriptorSet descriptorSet
		) {
			var descriptor = VkHelpers.CreateDescriptorSet(
				platform,
				program,
				[ binding ],
				DescriptorType.StorageBuffer,
				ShaderStageFlags.All
			);

			descriptorSet = descriptor.Set;
		}

		internal static void UpdateDescriptorSet(
			VkPlatform platform,
			DescriptorSet descriptorSet,
			uint binding,
			uint size,
			Buffer buffer
		) {
			var device = platform.PrimaryDevice.Logical;
			
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
				DescriptorType = DescriptorType.StorageBuffer,
				DescriptorCount = 1,
				PBufferInfo = &descriptorBufferInfo
			};
			
			platform.API.UpdateDescriptorSets(
				device,
				1,
				writeDescriptorSet,
				0,
				null
			);
		}
		
		/*internal static int Create(
			VkPlatform platform,
			VkShaderProgram program,
			uint binding,
			uint size,
			ref Buffer buffer,
			ref DeviceMemory bufferMemory
		) {
			Debug.Assert(platform.PrimaryDevice is not null);
			var device = platform.PrimaryDevice.Logical;
			
			Debug.Assert(bufferMemory.Handle == 0);
			VkHelpers.AllocateBuffer(platform, size, ref buffer, ref bufferMemory);
			
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
			return program.DescriptorSets.Count - 1;
		}*/
	}
}
