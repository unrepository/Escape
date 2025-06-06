using System.Diagnostics;
using System.Runtime.CompilerServices;
using GENESIS.GPU.Shader;
using NLog;
using Silk.NET.Core;
using Silk.NET.OpenGL;

namespace GENESIS.GPU.OpenGL {
	
	public class GLShader : IShader {

		public ShaderType Type { get; }
		public string Code { get; }
		
		public uint Id { get; private set; }

		public List<uint> DeallocatedDataObjects { get; } = [];

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly GLPlatform _platform;

		public GLShader(GLPlatform platform, ShaderType type, string code) {
			Type = type;
			Code = code;
			
			_platform = platform;
		}
		
		public uint Compile() {
			Debug.Assert(Id == 0);
			
			if(string.IsNullOrEmpty(Code)) {
				throw new ArgumentNullException(nameof(Code), "Cannot compile an empty shader!");
			}

			Id = _platform.API.CreateShader(Type);

			if(Id == 0) {
				throw new PlatformException("Failed to create a GL shader");
			}
			
			_platform.API.ShaderSource(Id, Code);
			_platform.API.CompileShader(Id);

			if(_platform.API.GetShader(Id, GLEnum.CompileStatus) == 0) {
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

			return Id;
		}

		public unsafe void PushData<T>(ShaderData<T> data) {
			if(data.Owner != this) {
				throw new InvalidOperationException("ShaderData is already assigned to an existing shader");
			}
			
			data.Owner = this;

			if(data.Id == 0) {
				data.Id = _platform.API.GenBuffer();
			}
			
			if(data.MappedMemory is not null) {
				return;
			}
			
			_platform.API.BindBuffer(BufferTargetARB.ShaderStorageBuffer, data.Id);
			
			//if(data.MappedMemory is null) {
			if(data is ShaderArrayData<T> arrayData) {
				fixed(void* dataPtr = &arrayData.InnerData[0]) {
					//_platform.API.NamedBufferStorage(data.Id, data.Size, dataPtr, BufferStorageMask.DynamicStorageBit);
					_platform.API.BufferData(BufferTargetARB.ShaderStorageBuffer, data.Size, dataPtr, BufferUsageARB.StaticDraw);
				}
			} else {
				fixed(void* dataPtr = &data.InnerData) {
					//_platform.API.NamedBufferStorage(data.Id, data.Size, dataPtr, BufferStorageMask.DynamicStorageBit);
					_platform.API.BufferData(BufferTargetARB.ShaderStorageBuffer, data.Size, dataPtr, BufferUsageARB.StaticDraw);
				}
			}
			
			_platform.API.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, data.Binding, data.Id);
			
			data.MappedMemory = (T*) _platform.API.MapBuffer(BufferTargetARB.ShaderStorageBuffer, BufferAccessARB.ReadWrite);
			/*} else {
				fixed(void* dataPtr = &data.Data) {
					Unsafe.CopyBlockUnaligned(data.MappedMemory, dataPtr, data.Size);
				}
			}*/
		}

		public unsafe void ReadData<T>(ref ShaderData<T> data) {
			Debug.Assert(data.MappedMemory is not null);
			data.Data = *data.MappedMemory;
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			_platform.API.DeleteShader(Id);
			Id = 0;
		}
	}
}
