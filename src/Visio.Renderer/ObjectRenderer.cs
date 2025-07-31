using System.Numerics;
using Visio.Renderer.Shader;
using Visio.Renderer.Shader.Pipelines;
using Visio.Renderer.Vulkan;

namespace Visio.Renderer {
	
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
				_ => throw new NotImplementedException("PlatformImpl")
			};
		}
	}
}
