using System.Numerics;
using Escape.Renderer.Shader.Pipelines;

namespace Escape.Renderer.OpenGL {
	
	public class GLObjectRenderer : ObjectRenderer {
		
		public GLObjectRenderer(string id, DefaultSceneShaderPipeline shaderPipeline) : base(id, shaderPipeline) { }
		public override bool AddObject(RenderableObject @object, Matrix4x4? matrix = null)
			=> throw new NotImplementedException();
		public override bool SetMatrix(RenderableObject @object, Matrix4x4 matrix)
			=> throw new NotImplementedException();
		public override bool RemoveObject(RenderableObject @object)
			=> throw new NotImplementedException();
		public override void Render(RenderQueue queue, TimeSpan delta) {
			throw new NotImplementedException();
		}
		public override void Reset() {
			throw new NotImplementedException();
		}
	}
}
