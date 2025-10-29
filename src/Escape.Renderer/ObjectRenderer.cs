using System.Numerics;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Shader.Pipelines;
using Escape.Renderer.Vulkan;
using Escape.Renderer.Shader;

namespace Escape.Renderer {
	
	public abstract class ObjectRenderer : IRenderer {

		public string Id { get; }
		public int Priority { get; init; } = 1000;
		
		public IShaderPipeline ShaderPipeline { get; }
		
		public ObjectRenderer(string id, IShaderPipeline shaderPipeline) {
			Id = id;
			ShaderPipeline = shaderPipeline;
		}

		public abstract bool AddObject(Renderable obj, Matrix4x4? matrix = null, Action<RenderQueue, TimeSpan>? renderCallback = null);
		public abstract bool SetMatrix(Renderable obj, Matrix4x4 matrix);
		public abstract bool RemoveObject(Renderable obj);
		
		public abstract void Render(RenderQueue queue, TimeSpan delta);
		public abstract void Reset();
		
		public static ObjectRenderer Create(IPlatform platform, IShaderPipeline shaderPipeline) {
			return platform switch {
				VkPlatform vkPlatform => new VkObjectRenderer("main", shaderPipeline),
				GLPlatform glPlatform => new GLObjectRenderer("main", shaderPipeline),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
	}
}
