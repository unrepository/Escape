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

		public GLShader(GLPlatform platform, ShaderType type, string code) : base(platform, type, code) {
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

		public override void Dispose() {
			GC.SuppressFinalize(this);
			_platform.API.DeleteShader(Handle);
			Handle = 0;
		}
	}
}
