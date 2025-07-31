using System.Diagnostics;
using Silk.NET.OpenGL;
using Visio.Renderer.Shader;
using Buffer = System.Buffer;

namespace Visio.Renderer.OpenGL {
	
	public class GLShaderArrayData<T> : IShaderArrayData<T> {
		
		public ulong Handle { get; private set; }
		
		public uint Binding { get; init; }

		public T[]? Data {
			get => _data;
			set => _data = value;
		}
		
		public uint Size { get; set; }
		
		public bool IsDirty { get; set; }

		private readonly GLPlatform _platform;
		private T[]? _data;
		
		public GLShaderArrayData(GLPlatform platform) {
			_platform = platform;
		}

		public unsafe void Push() {
			if(Handle == 0) Handle = _platform.API.CreateBuffer();

			void* dataPtr = null;
			
			if(Data != null && _data?.Length > 0) {
				fixed(void* ptr = &_data[0]) {
					dataPtr = ptr;
				}
			}
			
			_platform.API.NamedBufferData((uint) Handle, Size, dataPtr, VertexBufferObjectUsage.DynamicDraw);
			_platform.API.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, Binding, (uint) Handle);
		}

		public unsafe void Read() {
			Debug.Assert(Handle != 0);

			void* ptr = null;
			_platform.API.GetNamedBufferSubData((uint) Handle, 0, Size, ptr);

			fixed(void* dataPtr = &_data[0]) {
				Buffer.MemoryCopy(ptr, dataPtr, Size, Size);
			}
		}

		public void Write(uint offset, T[] data, uint? size = null) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			if(Handle != 0) _platform.API.DeleteBuffer((uint) Handle);
			Handle = 0;
		}
	}
}
