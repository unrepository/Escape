using System.Diagnostics;
using System.Runtime.CompilerServices;
using Escape.Renderer.Shader;
using NLog;
using Silk.NET.Core;
using Silk.NET.OpenGL;

namespace Escape.Renderer.OpenGL {
	
	public class GLShader : Shader.Shader {

		internal new uint Handle {
			get => (uint) base.Handle;
			set => base.Handle = value;
		}
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly GLPlatform _platform;

		public GLShader(GLPlatform platform, Family type, string code) : base(platform, type, code) {
			_platform = platform;
		}
		
		public override ulong Compile() {
			Debug.Assert(Handle == 0);
			
			if(string.IsNullOrEmpty(Code)) {
				throw new ArgumentNullException(nameof(Code), "Cannot compile an empty shader!");
			}

			Handle = _platform.API.CreateShader(Type switch {
				Family.Vertex => ShaderType.VertexShader,
				Family.Fragment => ShaderType.FragmentShader,
				Family.Compute => ShaderType.ComputeShader,
				Family.Geometry => ShaderType.GeometryShader,
				Family.TessellationControl => ShaderType.TessControlShader,
				Family.TessellationEvaluation => ShaderType.TessEvaluationShader
			});

			if(Handle == 0) {
				throw new PlatformException("Failed to create a GL shader");
			}
			
			_platform.API.ShaderSource(Handle, Code);
			_platform.API.CompileShader(Handle);

			if(_platform.API.GetShader(Handle, GLEnum.CompileStatus) == 0) {
				_logger.Fatal("Exception occurred while compiling shader: {InfoLog}", _platform.API.GetShaderInfoLog(Handle));
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
