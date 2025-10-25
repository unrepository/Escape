using System.Diagnostics;
using Escape.Renderer.Shader;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Escape.Renderer.OpenGL {
	
	public class GLRenderPipeline : RenderPipeline {

		public Action<GLRenderPipeline, GLPlatform>? StateSetup { get; set; } = (_, p) => { p.API.CullFace(TriangleFace.Back); };
		public List<EnableCap> EnableCaps { get; set; } = [ EnableCap.DepthTest ];
		public List<EnableCap> DisableCaps { get; set; } = [];
		
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
			
			// state setup
			StateSetup?.Invoke(this, _platform);

			foreach(var cap in EnableCaps) {
				_platform.API.Enable(cap);
			}
			
			foreach(var cap in DisableCaps) {
				_platform.API.Disable(cap);
			}

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
