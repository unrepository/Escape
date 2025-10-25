using System.Diagnostics;
using System.Runtime.InteropServices;
using Escape.Renderer.Shader;
using Silk.NET.OpenGL;
using Buffer = System.Buffer;

namespace Escape.Renderer.OpenGL {
	
	public class GLShaderData<T> : IShaderData<T> {
		
		public ulong Handle { get; private set; }
		public string Name { get; }
		public uint Binding { get; }

		public T? Data {
			get => _data;
			set {
				IsDirty = true;
				_data = value;
			}
		}
		
		public uint Size { get; set; }
		public bool IsDirty { get; set; }

		private readonly GLPlatform _platform;
		private T? _data;
		
		public GLShaderData(GLPlatform platform, ShaderProgram program, string name, uint binding, T? data, uint size) {
			_platform = platform;

			Name = name;
			Binding = binding;
			Data = data;
			Size = size;
			
			Handle = _platform.API.GenBuffer();
			
			_platform.API.BindBuffer(BufferTargetARB.UniformBuffer, (uint) Handle);
			_platform.API.UniformBlockBinding(program.Handle, _platform.API.GetUniformBlockIndex(program.Handle, name), Binding);
		}

		public unsafe void Push() {
			Debug.Assert(Handle != 0);
			if(!IsDirty) return;
			
			void* dataPtr = null;
			
			fixed(void* ptr = &_data) {
				dataPtr = ptr;
			}
			
			_platform.API.BindBuffer(BufferTargetARB.UniformBuffer, (uint) Handle);

			uint realSize = Size > 0 ? Size : (uint) sizeof(T);
			_platform.API.BufferData(BufferTargetARB.UniformBuffer, realSize, dataPtr, BufferUsageARB.StaticDraw);
			_platform.API.BindBufferBase(BufferTargetARB.UniformBuffer, Binding, (uint) Handle);
		}

		public unsafe void Read() {
			throw new NotImplementedException();
			/*Debug.Assert(Handle != 0);

			void* ptr = null;
			_platform.API.GetNamedBufferSubData((uint) Handle, 0, Size, ptr);

			fixed(void* dataPtr = &_data) {
				Buffer.MemoryCopy(ptr, dataPtr, Size, Size);
			}*/
		}

		public void Write(uint offset, T data, uint? size = null) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			if(Handle != 0) _platform.API.DeleteBuffer((uint) Handle);
			Handle = 0;
		}
	}
}
