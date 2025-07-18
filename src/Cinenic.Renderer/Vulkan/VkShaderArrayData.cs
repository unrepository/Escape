using System.Diagnostics;
using System.Runtime.InteropServices;
using Cinenic.Renderer.Shader;
using NLog;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Cinenic.Renderer.Vulkan {
	
	public unsafe class VkShaderArrayData<T> : IShaderArrayData<T> {
		
		public ulong Handle { get; private set; }
		public uint Binding { get; }
		
		public T[]? Data {
			get => _data;
			set => _data = value;
		}
		
		public uint Size { get; set; }

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly VkPlatform _platform;
		private readonly VkShaderProgram _program;

		private int _set;
		
		private T[]? _data;
		private void* _bufferDataPtr = null;
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

			Binding = binding;
			Data = data;
			Size = size;

			if(Size > 0) {
				_set = VkShaderData<T>.Create(
					_platform,
					_program,
					Binding,
					Size,
					ref _buffer,
					ref _bufferMemory
				);

				_bufferSize = Size;
			} else {
				_logger.Warn("Size == 0; will not allocate memory and descriptors which might lead to unknown errors!");
			}
		}
		
		public void Push() {
			Debug.Assert(_platform.PrimaryDevice is not null);
			
			// resize if current buffer is too small
			if(Size > _bufferSize) {
				Debug.Assert(Size > 0);

				if(_bufferDataPtr is not null) _platform.API.UnmapMemory(_platform.PrimaryDevice.Logical, _bufferMemory);
				_platform.API.FreeMemory(_platform.PrimaryDevice.Logical, _bufferMemory, null);
				_bufferMemory = default;
				
				VkShaderData<T>.Update(
					_platform,
					_program,
					_set,
					Binding,
					Size,
					ref _buffer,
					ref _bufferMemory
				);

				_bufferSize = Size;
			}

			if(Data is null || _data?.Length <= 0) {
				return;
			}

			fixed(void* dataPtr = _data) {
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

			_bufferMemory = default;
			_buffer = default;
		}
	}
}
