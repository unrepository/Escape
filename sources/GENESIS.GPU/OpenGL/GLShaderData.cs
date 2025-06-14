using System.Diagnostics;
using System.Runtime.InteropServices;
using GENESIS.GPU.Shader;
using Silk.NET.OpenGL;
using Buffer = System.Buffer;

namespace GENESIS.GPU.OpenGL {
	
	public class GLShaderData<T> : IShaderData<T> {
		
		public uint Handle { get; private set; }
		
		public uint Binding { get; init; }

		public T? Data {
			get => _data;
			set => _data = value;
		}
		
		public uint Size { get; set; }

		private readonly GLPlatform _platform;
		private T? _data;
		
		public GLShaderData(GLPlatform platform) {
			_platform = platform;
		}

		public unsafe void Push() {
			if(Handle == 0) Handle = _platform.API.CreateBuffer();

			void* dataPtr = null;
			
			//if(Data is not null) {
				fixed(void* ptr = &_data) {
					dataPtr = ptr;
				}
			//}
			
			_platform.API.NamedBufferData(Handle, Size, dataPtr, VertexBufferObjectUsage.DynamicDraw);
			_platform.API.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, Binding, Handle);
		}

		public unsafe void Read() {
			Debug.Assert(Handle != 0);

			void* ptr = null;
			_platform.API.GetNamedBufferSubData(Handle, 0, Size, ptr);

			fixed(void* dataPtr = &_data) {
				Buffer.MemoryCopy(ptr, dataPtr, Size, Size);
			}
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			if(Handle != 0) _platform.API.DeleteBuffer(Handle);
			Handle = 0;
		}
	}
}
