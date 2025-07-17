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
		
		private T[]? _data;
		private void* _bufferDataPtr = null;
		private uint _lastSize;

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
			Size = _lastSize = size;
			
			if(data is not null) {
				Size = _lastSize = size;
			
				VkShaderData<T>.Create(
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
				VkShaderData<T>.Create(
					_platform,
					_program,
					Binding,
					Size,
					ref _buffer,
					ref _bufferMemory
				);
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
	}
}
