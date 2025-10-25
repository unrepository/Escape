using System.Diagnostics;
using Escape.Renderer.Shader;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Escape.Renderer.OpenGL {
	
	public class GLRenderPipeline : RenderPipeline {

		private readonly GLPlatform _platform;

		public GLRenderPipeline(GLPlatform platform, RenderQueue queue, IShaderPipeline shaderPipeline)
			: base(platform, queue, shaderPipeline)
		{
			_platform = platform;
		}
		
		public override bool Begin() {
			Debug.Assert(Queue is GLRenderQueue);
			Debug.Assert(Queue.RenderTarget is not null);

			if(!Queue.Begin()) return false;
			Program.Get().Program!.Bind(this);
			
			// TODO per pipeline GL states
			_platform.API.Enable(EnableCap.DepthTest);
			_platform.API.CullFace(TriangleFace.Back);

			return true;
		}

		public override bool End() {
			return Queue.End();
		}
		
		public override void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
