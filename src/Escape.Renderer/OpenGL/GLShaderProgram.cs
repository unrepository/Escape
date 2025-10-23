using System.Diagnostics;
using Escape.Renderer.Shader;
using Silk.NET.Core;
using Silk.NET.OpenGL;

namespace Escape.Renderer.OpenGL {
	
	public class GLShaderProgram : ShaderProgram {
		
		private readonly GLPlatform _platform;
		private static uint _emptyVao = 0;
		
		public GLShaderProgram(GLPlatform platform, params Shader.Shader[] shaders) : base(platform, shaders) {
			_platform = platform;
		}

		public override void Bind(RenderPipeline pipeline) {
			if(Handle == 0) Build();
			
			if(_emptyVao == 0) _emptyVao = _platform.API.GenVertexArray();
			_platform.API.BindVertexArray(_emptyVao);
			
			_platform.API.UseProgram(Handle);

			foreach(var shader in Shaders) {
				foreach(var id in shader.DeallocatedDataObjects) {
					//_platform.API.BindBuffer(BufferTargetARB.ShaderStorageBuffer, id);
					//_platform.API.UnmapBuffer(BufferTargetARB.ShaderStorageBuffer);
					//_platform.API.UnmapNamedBuffer(id);
					//_platform.API.BindBuffer(BufferTargetARB.ShaderStorageBuffer, 0);
					_platform.API.DeleteBuffer(id);
				}
				
				shader.DeallocatedDataObjects.Clear();
			}
		}

		public override uint Build() {
			Debug.Assert(Handle == 0);
			
			if(Shaders.Length == 0) {
				throw new ArgumentException("A shader program must have at least 1 shader", nameof(Shaders));
			}

			Handle = _platform.API.CreateProgram();
			
			if(Handle == 0) {
				throw new PlatformException("Failed to create a GL shader program");
			}

			foreach(var shader in Shaders) {
				if(shader.Handle == 0) {
					shader.Compile();
				}
				
				_platform.API.AttachShader(Handle, (uint) shader.Handle);
			}
			
			_platform.API.LinkProgram(Handle);

			if(_platform.API.GetProgram(Handle, GLEnum.LinkStatus) == 0) {
				throw new PlatformException("Shader program linking failed");
			}
			
			// cleanup
			foreach(var shader in Shaders) {
				_platform.API.DetachShader(Handle, (uint) shader.Handle);
				shader.Dispose();
			}

			return Handle;
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
			_platform.API.DeleteProgram(Handle);
			Handle = 0;
		}
	}
}
