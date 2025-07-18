using System.Diagnostics;
using System.Numerics;
using Cinenic.Renderer.Shader;
using Cinenic.Renderer.Shader.Pipelines;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkObjectRenderer : ObjectRenderer {
		
		public VkObjectRenderer(string id, DefaultSceneShaderPipeline shaderPipeline) : base(id, shaderPipeline) { }

		public unsafe override void Render(RenderQueue queue, TimeSpan delta) {
			Debug.Assert(queue is VkRenderQueue);
			var vkQueue = (VkRenderQueue) queue;
			var vkPlatform = (VkPlatform) queue.Platform;
			
			Debug.Assert(Models.Count == Matrices.Count, "Model-Matrix list size mismatch! This should never happen!");
			
			for(int i = 0; i < Models.Count; i++) {
				var model = Models[i];
				var matrix = Matrices[i];
				
				foreach(var mesh in model.Meshes) {
					ShaderPipeline.VertexData.Data = mesh.Vertices;
					ShaderPipeline.VertexData.Size = (uint) (mesh.Vertices.Length * sizeof(Vertex));
					
					ShaderPipeline.IndexData.Data = mesh.Indices;
					ShaderPipeline.IndexData.Size = (uint) (mesh.Indices.Length * sizeof(uint));
					
					ShaderPipeline.MaterialData.Data = mesh.Material;
					ShaderPipeline.MaterialData.Size = (uint) sizeof(Material);
					
					ShaderPipeline.MatrixData.Data = matrix;
					ShaderPipeline.MaterialData.Size = (uint) sizeof(Matrix4x4);
					
					ShaderPipeline.PushData();
					
					vkPlatform.API.CmdDraw(
						vkQueue.CommandBuffer,
						(uint) mesh.Indices.Length,
						1,
						0,
						0
					);
				}
			}
		}

		public override void Reset() {
			Models.Clear();
			Matrices.Clear();
		}
	}
}
