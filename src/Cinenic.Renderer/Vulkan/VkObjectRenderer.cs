using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Cinenic.Renderer.Shader;
using Cinenic.Renderer.Shader.Pipelines;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkObjectRenderer : ObjectRenderer {
		
		public VkObjectRenderer(string id, DefaultSceneShaderPipeline shaderPipeline) : base(id, shaderPipeline) { }

		public unsafe override void Render(RenderQueue queue, TimeSpan delta) {
			Debug.Assert(queue is VkRenderQueue);
			var vkQueue = (VkRenderQueue) queue;
			var vkPlatform = (VkPlatform) queue.Platform;
			
			Debug.Assert(Models.Count == Matrices.Count, "Model-Matrix list size mismatch! This should never happen!");
			
			uint vertexOffset = 0;
			uint indexOffset = 0;
			uint materialOffset = 0;
			uint matrixOffset = 0;
			
			for(int i = 0; i < Models.Count; i++) {
				var model = Models[i];
				var matrix = Matrices[i];
				
				foreach(var mesh in model.Meshes) {
					var vertexDataSize = (uint) (mesh.Vertices.Length * sizeof(Vertex));
					var indexDataSize = (uint) (mesh.Indices.Length * sizeof(uint));
					var materialDataSize = (uint) sizeof(Material.Data);
					var matrixDataSize = (uint) sizeof(Material.Data);
					
					ShaderPipeline.VertexData.Size = vertexOffset + vertexDataSize;
					ShaderPipeline.VertexData.Write(vertexOffset, mesh.Vertices);
					
					ShaderPipeline.IndexData.Size = indexOffset + indexDataSize;
					ShaderPipeline.IndexData.Write(indexOffset, mesh.Indices);
					
					ShaderPipeline.MaterialData.Size = materialOffset + materialDataSize;
					ShaderPipeline.MaterialData.Write(materialOffset, mesh.Material.CreateData());
					
					ShaderPipeline.MatrixData.Size = matrixOffset + matrixDataSize;
					ShaderPipeline.MatrixData.Write(matrixOffset, matrix);
					
					//ShaderPipeline.PushData();

					var pc = new PushConstants {
						VertexOffset = vertexOffset / (uint) sizeof(Vertex),
						IndexOffset = indexOffset / (uint) sizeof(uint),
						MaterialOffset = materialOffset / (uint) sizeof(Material.Data),
						MatrixOffset = matrixOffset / (uint) sizeof(Matrix4x4)
					};
					
					vkPlatform.API.CmdPushConstants(
						vkQueue.CommandBuffer,
						((VkRenderPipeline) queue.Pipeline!).PipelineLayout,
						ShaderStageFlags.VertexBit,
						0,
						(uint) sizeof(PushConstants),
						&pc
					);
					
					// mesh.Material.AlbedoTexture?.Bind(queue, (uint) Material.TextureType.Albedo);
					// mesh.Material.NormalTexture?.Bind(queue, (uint) Material.TextureType.Normal);
					// mesh.Material.MetallicTexture?.Bind(queue, (uint) Material.TextureType.Metallic);
					// mesh.Material.RoughnessTexture?.Bind(queue, (uint) Material.TextureType.Roughness);
					
					// fixed(DescriptorSet* descriptorSetsPtr = ((VkShaderProgram) vkQueue.Pipeline.ShaderPipeline.Program)._descriptorSetsArray) {
					// 	vkPlatform.API.CmdBindDescriptorSets(
					// 		vkQueue.CommandBuffer,
					// 		queue.Type switch {
					// 			RenderQueue.Family.Graphics => PipelineBindPoint.Graphics,
					// 			RenderQueue.Family.Compute => PipelineBindPoint.Compute,
					// 			_ => throw new NotImplementedException()
					// 		},
					// 		((VkRenderPipeline) queue.Pipeline).PipelineLayout,
					// 		0,
					// 		(uint) ((VkShaderProgram) vkQueue.Pipeline.ShaderPipeline.Program)._descriptorSetsArray.Length,
					// 		descriptorSetsPtr,
					// 		0,
					// 		null
					// 	);
					// }
					
					vkPlatform.API.CmdDraw(
						vkQueue.CommandBuffer,
						(uint) mesh.Indices.Length,
						1,
						0,
						0
					);
					
					/*vkPlatform.API.CmdDrawIndexed(
						vkQueue.CommandBuffer,
						(uint) mesh.Indices.Length,
						1,
						/*indexOffset / sizeof(uint)#1# 0,
						/*(int) (vertexOffset / sizeof(Vertex))#1# 0,
						0
					);*/

					vertexOffset += vertexDataSize;
					indexOffset += indexDataSize;
					materialOffset += materialDataSize;
					matrixOffset += matrixDataSize;
				}
			}
			
			//ShaderPipeline.VkBindTextureDescriptors(vkQueue);
		}

		public override void Reset() {
			Models.Clear();
			Matrices.Clear();
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct PushConstants {
			
			[FieldOffset(0)] public uint VertexOffset;
			[FieldOffset(4)] public uint IndexOffset;
			[FieldOffset(8)] public uint MaterialOffset;
			[FieldOffset(12)] public uint MatrixOffset;
		}
	}
}
