using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

namespace GENESIS.GPU.Compute {
	
	public class ShaderCompute {
		
		public ShaderProgram Program { get; }
		public Framebuffer Framebuffer { get; }

		public Vector2D<int> Size => (Vector2D<int>) Framebuffer.Size;
		public uint VertexCount { get; set; } = 6;

		private readonly IPlatform _platform;

		public ShaderCompute(IPlatform platform, ShaderProgram program, Vector2D<uint> framebufferSize) {
			_platform = platform;
			Program = program;

			var vertexShaderCode = platform switch {
				GLPlatform => Assembly.GetExecutingAssembly().ReadTextResourceN("Shaders.OpenGL.compute.vert"),
				_ => throw new NotImplementedException()
			};

			bool hasVertexShader = Program.Shaders.Any(shader => shader.Type == ShaderType.VertexShader);
			if(!hasVertexShader) {
				if(Program.Id != 0) Program.Dispose();
				
				Program = ShaderProgram.Create(platform, program.Shaders.Append(Shader.Shader.Create(
					platform,
					ShaderType.VertexShader,
					vertexShaderCode
				)).ToArray());
			}
			
			Framebuffer = Framebuffer.Create(platform, framebufferSize);
		}

		public void Render() {
			Program.Bind();
			Framebuffer.Bind();

			switch(_platform) {
				case GLPlatform glPlatform:
					glPlatform.API.DrawArrays(GLEnum.Triangles, 0, VertexCount);
					break;
				default:
					throw new NotImplementedException();
			}
			
			Framebuffer.Unbind();
		}

		public ReadOnlySpan<Rgba32> ReadOutput(int attachment = 0, Rectangle<uint>? area = null) {
			var output = Framebuffer.Read(attachment, area);
			return MemoryMarshal.Cast<byte, Rgba32>(output);
		}
	}
}
