using System.Diagnostics;
using GENESIS.GPU.Shader;
using Silk.NET.Core;
using Silk.NET.OpenGL;

namespace GENESIS.GPU.OpenGL {
	
	public class GLShaderProgram : IShaderProgram {
		
		public IShader[] Shaders { get; }
		
		public uint Id { get; private set; }
		
		private readonly GLPlatform _platform;
		private static uint _emptyVao = 0;
		
		public GLShaderProgram(GLPlatform platform, params IShader[] shaders) {
			Shaders = shaders;
			_platform = platform;
		}

		public void Bind() {
			if(Id == 0) Build();
			
			if(_emptyVao == 0) _emptyVao = _platform.API.GenVertexArray();
			_platform.API.BindVertexArray(_emptyVao);
			
			_platform.API.UseProgram(Id);

			foreach(var shader in Shaders) {
				foreach(var id in shader.DeallocatedDataObjects) {
					_platform.API.BindBuffer(BufferTargetARB.ShaderStorageBuffer, id);
					_platform.API.UnmapBuffer(BufferTargetARB.ShaderStorageBuffer);
					_platform.API.BindBuffer(BufferTargetARB.ShaderStorageBuffer, 0);
					_platform.API.DeleteBuffer(id);
				}
			}
		}

		public uint Build() {
			Debug.Assert(Id == 0);
			
			if(Shaders.Length == 0) {
				throw new ArgumentException("A shader program must have at least 1 shader", nameof(Shaders));
			}

			Id = _platform.API.CreateProgram();
			
			if(Id == 0) {
				throw new PlatformException("Failed to create a GL shader program");
			}

			foreach(var shader in Shaders) {
				if(shader.Id == 0) {
					shader.Compile();
				}
				
				_platform.API.AttachShader(Id, shader.Id);
			}
			
			_platform.API.LinkProgram(Id);

			if(_platform.API.GetProgram(Id, GLEnum.LinkStatus) == 0) {
				throw new PlatformException("Shader program linking failed");
			}
			
			// cleanup
			foreach(var shader in Shaders) {
				_platform.API.DetachShader(Id, shader.Id);
				shader.Dispose();
			}

			return Id;
		}
		
		public void Dispose() {
			GC.SuppressFinalize(this);
			_platform.API.DeleteProgram(Id);
			Id = 0;
		}
	}
}
