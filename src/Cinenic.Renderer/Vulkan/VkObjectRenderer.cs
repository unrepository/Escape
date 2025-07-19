using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer.Shader;
using Cinenic.Renderer.Shader.Pipelines;
using Silk.NET.Vulkan;

using static Cinenic.Renderer.Vulkan.VkHelpers;

namespace Cinenic.Renderer.Vulkan {
	
	public class VkObjectRenderer : ObjectRenderer {
		
		private List<RenderableModel> _models = [];
		
		private List<Vertex> _totalVertices = [];
		private List<uint> _totalIndices = [];
		private List<Material.Data> _totalMaterials = [];
		private List<Matrix4x4> _totalMatrices = [];

		private DescriptorSet _textureDescriptorSet;
		private Dictionary<int, VkTexture> _textureIndexMap = [];

		public VkObjectRenderer(string id, DefaultSceneShaderPipeline shaderPipeline) : base(id, shaderPipeline) {
			var descriptor = CreateDescriptorSet(
				(VkPlatform) shaderPipeline.Platform,
				(VkShaderProgram) shaderPipeline.Program,
				[0],
				1024,
				DescriptorType.CombinedImageSampler,
				ShaderStageFlags.FragmentBit
			);

			_textureDescriptorSet = descriptor.Set;
		}

		public override void AddObject(RenderableModel model, Matrix4x4 matrix) {
			foreach(var mesh in model.Meshes) {
				_totalVertices.AddRange(mesh.Vertices);
				_totalIndices.AddRange(mesh.Indices);
				_totalMaterials.Add(mesh.Material.CreateData());
				
				// if(mesh.Material.AlbedoTexture is VkTexture albedoTexture) _textureIndexMap.TryAdd(albedoTexture.Index, albedoTexture);
				// if(mesh.Material.NormalTexture is VkTexture normalTexture) _textureIndexMap.TryAdd(normalTexture.Index, normalTexture);
				// if(mesh.Material.MetallicTexture is VkTexture metallicTexture) _textureIndexMap.TryAdd(metallicTexture.Index, metallicTexture);
				// if(mesh.Material.RoughnessTexture is VkTexture roughnessTexture) _textureIndexMap.TryAdd(roughnessTexture.Index, roughnessTexture);
			}
			
			_totalMatrices.Add(matrix);
			_models.Add(model);
		}

		public unsafe override void Render(RenderQueue queue, TimeSpan delta) {
			Debug.Assert(queue is VkRenderQueue);
			var vkQueue = (VkRenderQueue) queue;
			var vkPlatform = (VkPlatform) queue.Platform;
			
			//Debug.Assert(Models.Count == Matrices.Count, "Model-Matrix list size mismatch! This should never happen!");

			// var allVertexData = new List<Vertex>();
			// var allIndexData = new List<uint>();
			// var allMaterialData = new List<Material.Data>();
			// var allMatrixData = new List<Matrix4x4>();
			//
			// for(int i = 0; i < Models.Count; i++) {
			// 	var model = Models[i];
			// 	var matrix = Matrices[i];
			//
			// 	foreach(var mesh in model.Meshes) {
			// 		allVertexData.AddRange(mesh.Vertices);
			// 		allIndexData.AddRange(mesh.Indices);
			// 		allMaterialData.Add(mesh.Material.CreateData());
			// 		allMatrixData.Add(matrix);
			// 	}
			// }

			ShaderPipeline.VertexData.Size = (uint) (_totalVertices.Count * sizeof(Vertex));
			ShaderPipeline.IndexData.Size = (uint) (_totalIndices.Count * sizeof(uint));
			ShaderPipeline.MaterialData.Size = (uint) (_totalMaterials.Count * sizeof(Material.Data));
			ShaderPipeline.MatrixData.Size = (uint) (_totalMatrices.Count * sizeof(Matrix4x4));

			ShaderPipeline.VertexData.Data = _totalVertices.ToArrayNoCopy();
			ShaderPipeline.IndexData.Data = _totalIndices.ToArrayNoCopy();
			ShaderPipeline.MaterialData.Data = _totalMaterials.ToArrayNoCopy();
			ShaderPipeline.MatrixData.Data = _totalMatrices.ToArrayNoCopy();

			ShaderPipeline.PushData();
			
			uint vertexOffset = 0;
			uint indexOffset = 0;
			uint materialOffset = 0;
			uint matrixOffset = 0;
			
			foreach(var model in _models) {
				foreach(var mesh in model.Meshes) {
					/*var vertexDataSize = (uint) (mesh.Vertices.Length * sizeof(Vertex));
					var indexDataSize = (uint) (mesh.Indices.Length * sizeof(uint));
					var materialDataSize = (uint) sizeof(Material.Data);
					var matrixDataSize = (uint) sizeof(Matrix4x4);
					
					ShaderPipeline.VertexData.Size = vertexOffset + vertexDataSize;
					ShaderPipeline.VertexData.Write(vertexOffset, mesh.Vertices);
					
					ShaderPipeline.IndexData.Size = indexOffset + indexDataSize;
					ShaderPipeline.IndexData.Write(indexOffset, mesh.Indices);
					
					ShaderPipeline.MaterialData.Size = materialOffset + materialDataSize;
					ShaderPipeline.MaterialData.Write(materialOffset, mesh.Material.CreateData());
					
					ShaderPipeline.MatrixData.Size = matrixOffset + matrixDataSize;
					ShaderPipeline.MatrixData.Write(matrixOffset, matrix);*/
					
					//ShaderPipeline.PushData();

					var pc = new PushConstants {
						VertexOffset = vertexOffset,
						IndexOffset = indexOffset,
						MaterialOffset = materialOffset,
						MatrixOffset = matrixOffset
					};

					if(mesh.Material.AlbedoTexture is VkTexture albedoTexture) pc.AlbedoTextureIndex = albedoTexture.Index;
					if(mesh.Material.NormalTexture is VkTexture normalTexture) pc.AlbedoTextureIndex = normalTexture.Index;
					if(mesh.Material.MetallicTexture is VkTexture metallicTexture) pc.AlbedoTextureIndex = metallicTexture.Index;
					if(mesh.Material.RoughnessTexture is VkTexture roughnessTexture) pc.AlbedoTextureIndex = roughnessTexture.Index;
					
					vkPlatform.API.CmdPushConstants(
						vkQueue.CommandBuffer,
						((VkRenderPipeline) queue.Pipeline!).PipelineLayout,
						ShaderStageFlags.All,
						0,
						(uint) sizeof(PushConstants),
						&pc
					);
					
					// this doesn't bind the textures per-se, but rather adds them to the textures descriptor array (if they aren't already)...
					mesh.Material.AlbedoTexture?.Bind(queue, 0);
					mesh.Material.NormalTexture?.Bind(queue, 0);
					mesh.Material.MetallicTexture?.Bind(queue, 0);
					mesh.Material.RoughnessTexture?.Bind(queue, 0);
					
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

					vertexOffset += (uint) mesh.Vertices.Length;
					indexOffset += (uint) mesh.Indices.Length;
					materialOffset += 1;
				}
				
				matrixOffset += 1;
			}
			
			//ShaderPipeline.VkBindTextureDescriptors(vkQueue);
			Reset();
		}

		public override void Reset() {
			_models.Clear();
			_totalVertices.Clear();
			_totalIndices.Clear();
			_totalMaterials.Clear();
			_totalMatrices.Clear();
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct PushConstants {
			
			[FieldOffset(0)] public uint VertexOffset;
			[FieldOffset(4)] public uint IndexOffset;
			[FieldOffset(8)] public uint MaterialOffset;
			[FieldOffset(12)] public uint MatrixOffset;
			
			[FieldOffset(16)] public int AlbedoTextureIndex;
			[FieldOffset(20)] public int NormalTextureIndex;
			[FieldOffset(24)] public int MetallicTextureIndex;
			[FieldOffset(28)] public int RoughnessTextureIndex;
		}
	}
}
