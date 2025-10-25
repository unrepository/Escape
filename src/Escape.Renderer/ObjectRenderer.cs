using System.Numerics;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Shader.Pipelines;
using Escape.Renderer.Vulkan;
using Escape.Renderer.Shader;

namespace Escape.Renderer {
	
	public abstract class ObjectRenderer : IRenderer {

		public string Id { get; }
		public int Priority { get; init; } = 1000;
		
		public DefaultSceneShaderPipeline ShaderPipeline { get; }
		
		public ObjectRenderer(string id, DefaultSceneShaderPipeline shaderPipeline) {
			Id = id;
			ShaderPipeline = shaderPipeline;
		}

		public abstract bool AddObject(RenderableObject @object, Matrix4x4? matrix = null);
		public abstract bool SetMatrix(RenderableObject @object, Matrix4x4 matrix);
		public abstract bool RemoveObject(RenderableObject @object);
		
		public abstract void Render(RenderQueue queue, TimeSpan delta);
		public abstract void Reset();
		
		public static ObjectRenderer Create(IPlatform platform, DefaultSceneShaderPipeline shaderPipeline) {
			return platform switch {
				VkPlatform vkPlatform => new VkObjectRenderer("main", shaderPipeline),
				GLPlatform glPlatform => new GLObjectRenderer("main", shaderPipeline),
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
	}
}
