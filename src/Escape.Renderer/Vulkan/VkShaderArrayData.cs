using System.Diagnostics;
using System.Runtime.InteropServices;
using Escape.Renderer.Shader;
using Escape.Extensions.CSharp;
using NLog;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

using static Escape.Renderer.Vulkan.VkHelpers;

namespace Escape.Renderer.Vulkan {
	
	public unsafe class VkShaderArrayData<T> : IShaderArrayData<T> {
		
		public ulong Handle { get; private set; }
		public string Name => throw new NotSupportedException();
		public uint Binding { get; }
		
		public T[]? Data {
			get => _data;
			set {
				IsDirty = true;
				_data = value;
			}
		}
		
		public uint Size {
			get;
			set {
				IsDirty = true;
				
				if(_bufferSize == 0) {
					field = value;
					return;
				}
				
				//Debug.Assert(value > 0);
				if(value == 0) return;
				
				if(value > _bufferSize) {
					value = value.CeilIncrement(1024 * 1024);
				
					_logger.Trace("Buffer size changed ({OldSize} -> {NewSize}); reallocating", _bufferSize, value);

					if(_bufferDataPtr is not null) {
						_platform.API.UnmapMemory(_platform.PrimaryDevice.Logical, _bufferMemory);
					}
				
					_platform.API.FreeMemory(_platform.PrimaryDevice.Logical, _bufferMemory, null);
					_platform.API.DestroyBuffer(_platform.PrimaryDevice.Logical, _buffer, null);

					_bufferDataPtr = VkShaderData<T>.AllocateMemory(
						_platform,
						value,
						ref _buffer,
						ref _bufferMemory
					);
				
					VkShaderData<T>.UpdateDescriptorSet(
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

		public bool IsDirty { get; set; }

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private readonly VkPlatform _platform;
		private readonly VkShaderProgram _program;

		private readonly DescriptorSet _descriptorSet;
		
		private T[]? _data;
		private void* _bufferDataPtr;
		private uint _bufferSize;

		private Buffer _buffer;
		private DeviceMemory _bufferMemory;

		public VkShaderArrayData(
			VkPlatform platform,
			ShaderProgram program,
			uint binding,
			T[]? data,
			uint size
		) {
			_platform = platform;
			_program = (VkShaderProgram) program;

			Debug.Assert(size > 0);
			
			Binding = binding;
			Data = data;
			Size = size;
			
			_bufferDataPtr = VkShaderData<T>.AllocateMemory(
				_platform,
				Size,
				ref _buffer,
				ref _bufferMemory
			);

			VkShaderData<T>.CreateDescriptorSet(
				_platform,
				_program,
				binding,
				out _descriptorSet
			);
			
			VkShaderData<T>.UpdateDescriptorSet(
				_platform,
				_descriptorSet,
				binding,
				Size,
				_buffer
			);
			
			_bufferSize = Size;
		}
		
		public void Push() {
			if(!IsDirty) return;
			Debug.Assert(_bufferDataPtr is not null);

			if(Data is null || Size == 0 || _data?.Length <= 0) {
				return;
			}

			fixed(void* dataPtr = _data) {
				System.Buffer.MemoryCopy(dataPtr, _bufferDataPtr, Size, Size);
			}
		}
		
		public void Read() {
			throw new NotImplementedException();
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
	}
}
