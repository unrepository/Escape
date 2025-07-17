using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Shader;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;
using Framebuffer = Cinenic.Renderer.Framebuffer;
using Shader = Cinenic.Renderer.Shader.Shader;

namespace Cinenic.Compute {
	
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
				_ => throw new NotImplementedException() // PlatformImpl
			};

			bool hasVertexShader = Program.Shaders.Any(shader => shader.Type == Shader.Family.Vertex);
			if(!hasVertexShader) {
				if(Program.Handle != 0) Program.Dispose();
				
				Program = ShaderProgram.Create(platform, program.Shaders.Append(Shader.Create(
					platform,
					Shader.Family.Vertex,
					vertexShaderCode
				)).ToArray());
			}
			
			Framebuffer = Framebuffer.Create(platform, null /* TODO */, framebufferSize);
		}

		public void Render() {
			Program.Bind(null /* TODO */);
			Framebuffer.Bind();

			switch(_platform) {
				case GLPlatform glPlatform:
					glPlatform.API.DrawArrays(GLEnum.Triangles, 0, VertexCount);
					break;
				default:
					throw new NotImplementedException(); // PlatformImpl
			}
			
			Framebuffer.Unbind();
		}

		public ReadOnlySpan<Rgba32> ReadOutput(int attachment = 0, Rectangle<uint>? area = null) {
			var output = Framebuffer.Read(attachment, area);
			return MemoryMarshal.Cast<byte, Rgba32>(output);
		}
	}
}
