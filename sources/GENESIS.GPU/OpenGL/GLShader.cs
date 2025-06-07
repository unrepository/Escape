using System.Diagnostics;
using System.Runtime.CompilerServices;
using GENESIS.GPU.Shader;
using NLog;
using Silk.NET.Core;
using Silk.NET.OpenGL;

namespace GENESIS.GPU.OpenGL {
	
	public class GLShader : Shader.Shader {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly GLPlatform _platform;

		public GLShader(GLPlatform platform, ShaderType type, string code) : base(type, code) {
			_platform = platform;
		}
		
		public override uint Compile() {
			Debug.Assert(Handle == 0);
			
			if(string.IsNullOrEmpty(Code)) {
				throw new ArgumentNullException(nameof(Code), "Cannot compile an empty shader!");
			}

			Handle = _platform.API.CreateShader(Type);

			if(Handle == 0) {
				throw new PlatformException("Failed to create a GL shader");
			}
			
			_platform.API.ShaderSource(Handle, Code);
			_platform.API.CompileShader(Handle);

			if(_platform.API.GetShader(Handle, GLEnum.CompileStatus) == 0) {
				_logger.Fatal("Exception occurred while compiling shader");
				_logger.Fatal("=== SHADER CODE BEGIN ===");

				{
					var c = Code.Split("\n");

					for(int i = 0; i < c.Length; i++) {
						_logger.Fatal($"{i + 1}: {c[i]}");
					}
				}
				
				_logger.Fatal("=== SHADER CODE END ===");
				
				throw new PlatformException("Shader compilation failed");
			}

			return Handle;
		}

		public unsafe override void PushData<T>(ShaderData<T> data) {
			if(data.Owner != this && data.Owner is not null) {
				throw new InvalidOperationException("ShaderData is already assigned to an existing shader");
			}
			
			data.Owner = this;

			if(data.Id != 0) {
				throw new InvalidOperationException("ShaderData already pushed to shader");
			}
			
			data.Id = _platform.API.CreateBuffer();

			if(data.Data is null) {
				// only allocate
				_platform.API.NamedBufferData(data.Id, data.Size, null, VertexBufferObjectUsage.DynamicDraw);
			} else {
				if(data is ShaderArrayData<T> arrayData) {
					if(arrayData.InnerData.Length == 0) {
						_platform.API.NamedBufferData(data.Id, data.Size, null, VertexBufferObjectUsage.DynamicDraw);
					} else {
						fixed(void* dataPtr = &arrayData.InnerData[0]) {
							_platform.API.NamedBufferData(data.Id, data.Size, dataPtr, VertexBufferObjectUsage.DynamicDraw);
						}
					}
				} else {
					fixed(void* dataPtr = &data.InnerData) {
						_platform.API.NamedBufferData(data.Id, data.Size, dataPtr, VertexBufferObjectUsage.DynamicDraw);
					}
				}
			}

			_platform.API.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, data.Binding, data.Id);
		}

		public unsafe override void UpdateData<T>(ShaderData<T> data) {
			if(data.Owner != this) {
				throw new InvalidOperationException("ShaderData is not bound to this shader");
			}

			if(data.Id == 0) {
				throw new InvalidOperationException("ShaderData does not yet exist");
			}
			
			if(data is ShaderArrayData<T> arrayData) {
				if(arrayData.InnerData.Length == 0) {
					_platform.API.NamedBufferSubData(data.Id, 0, data.Size, null);
					//_platform.API.NamedBufferData(data.Id, data.Size, null, VertexBufferObjectUsage.DynamicDraw);
				} else {
					fixed(void* dataPtr = &arrayData.InnerData[0]) {
						_platform.API.NamedBufferSubData(data.Id, 0, data.Size, dataPtr);
						//_platform.API.NamedBufferData(data.Id, data.Size, dataPtr, VertexBufferObjectUsage.DynamicDraw);
					}
				}
			} else {
				fixed(void* dataPtr = &data.InnerData) {
					_platform.API.NamedBufferSubData(data.Id, 0, data.Size, dataPtr);
					//_platform.API.NamedBufferData(data.Id, data.Size, dataPtr, VertexBufferObjectUsage.DynamicDraw);
				}
			}
		}

		public unsafe override void ReadData<T>(ref ShaderData<T> data) {
			Debug.Assert(data.MappedMemory is not null);
			data.Data = *data.MappedMemory;
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			_platform.API.DeleteShader(Handle);
			Handle = 0;
		}
	}
}
