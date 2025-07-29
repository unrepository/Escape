using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer.Shader;
using Cinenic.Renderer.Shader.Pipelines;
using NLog;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Vulkan {
	
	// TODO we could perchance optimize this with IShaderData.Write to only write parts of data in Add/RemoveObject, SetMatrix
	public class VkObjectRenderer : ObjectRenderer {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly ConcurrentQueue<ObjectData> _pendingAdd = new();
		private readonly ConcurrentQueue<RenderableObject> _pendingRemove = new();

		private readonly ConcurrentDictionary<RenderableObject, ObjectData> _objects = new();
		private readonly ConcurrentDictionary<RenderableObject, Matrix4x4> _matrices = new();

		public VkObjectRenderer(string id, DefaultSceneShaderPipeline shaderPipeline) : base(id, shaderPipeline) { }

		public override bool AddObject(RenderableObject obj, Matrix4x4? matrix = null) {
			if(_objects.ContainsKey(obj)) return false;

			matrix ??= Matrix4x4.Identity;

			var model = obj.Model;
			var vertices = model.Meshes.SelectMany(m => m.Vertices).ToArray();
			var indices = model.Meshes.SelectMany(m => m.Indices).ToArray();
			var materials = model.Meshes.Select(m => m.Material.CreateData()).ToArray();

			var data = new ObjectData(obj, model.Meshes, vertices, indices, materials);
			_pendingAdd.Enqueue(data);
			_matrices[obj] = matrix.Value;

			_logger.Trace("Added object {Id}", obj.Id);
			return true;
		}

		public override bool SetMatrix(RenderableObject obj, Matrix4x4 matrix) {
			return _matrices.TryUpdate(obj, matrix, _matrices.GetValueOrDefault(obj));
		}

		public override bool RemoveObject(RenderableObject obj) {
			if (!_objects.ContainsKey(obj)) return false;

			_pendingRemove.Enqueue(obj);
			_matrices.TryRemove(obj, out _);
			
			_logger.Trace("Removed object {Id}", obj.Id);
			return true;
		}

		public unsafe override void Render(RenderQueue queue, TimeSpan delta) {
			Debug.Assert(queue is VkRenderQueue);
			var vkQueue = (VkRenderQueue) queue;
			var vkPlatform = (VkPlatform) queue.Platform;

			// process queues
			while(_pendingAdd.TryDequeue(out var add)) {
				_objects[add.Object] = add;
			}
			while(_pendingRemove.TryDequeue(out var remove)) {
				_objects.TryRemove(remove, out _);
			}

			// construct buffer data
			var totalVertices = new List<Vertex>();
			var totalIndices = new List<uint>();
			var totalMaterials = new List<Material.Data>();
			var totalMatrices = new List<Matrix4x4>();
			var drawInstances = new List<DrawInstance>();

			uint vertexOffset = 0;
			uint indexOffset = 0;
			uint materialOffset = 0;
			uint matrixOffset = 0;

			foreach (var obj in _objects.Values) {
				var matrix = _matrices.GetValueOrDefault(obj.Object, Matrix4x4.Identity);

				totalVertices.AddRange(obj.Vertices);
				totalIndices.AddRange(obj.Indices);
				totalMaterials.AddRange(obj.Materials);
				totalMatrices.Add(matrix);

				drawInstances.Add(new(
					obj.Meshes,
					vertexOffset,
					indexOffset,
					materialOffset,
					matrixOffset
				));

				vertexOffset += (uint)obj.Vertices.Length;
				indexOffset += (uint)obj.Indices.Length;
				materialOffset += (uint)obj.Materials.Length;
				matrixOffset += 1;
			}

			// nothing to render
			if(totalVertices.Count < 0 || totalIndices.Count < 0) return;
			if(totalMaterials.Count < 0 || totalMatrices.Count < 0) return;

			// upload data to GPU
			ShaderPipeline.VertexData.Size = (uint) (totalVertices.Count * sizeof(Vertex));
			ShaderPipeline.IndexData.Size = (uint) (totalIndices.Count * sizeof(uint));
			ShaderPipeline.MaterialData.Size = (uint) (totalMaterials.Count * sizeof(Material.Data));
			ShaderPipeline.MatrixData.Size = (uint) (totalMatrices.Count * sizeof(Matrix4x4));

			ShaderPipeline.VertexData.Data = totalVertices.ToArrayNoCopy();
			ShaderPipeline.IndexData.Data = totalIndices.ToArrayNoCopy();
			ShaderPipeline.MaterialData.Data = totalMaterials.ToArrayNoCopy();
			ShaderPipeline.MatrixData.Data = totalMatrices.ToArrayNoCopy();

			ShaderPipeline.PushData();

			foreach (var instance in drawInstances) {
				foreach (var mesh in instance.Meshes) {
					var pc = new PushConstants {
						VertexOffset = instance.VertexOffset,
						IndexOffset = instance.IndexOffset,
						MaterialOffset = instance.MaterialOffset,
						MatrixOffset = instance.MatrixOffset
					};

					if(mesh.Material.AlbedoTexture?.Get().Texture is VkTexture albedo) pc.AlbedoTextureIndex = albedo.Index;
					if(mesh.Material.NormalTexture?.Get().Texture is VkTexture normal) pc.NormalTextureIndex = normal.Index;
					if(mesh.Material.MetallicTexture?.Get().Texture is VkTexture metallic) pc.MetallicTextureIndex = metallic.Index;
					if(mesh.Material.RoughnessTexture?.Get().Texture is VkTexture roughness) pc.RoughnessTextureIndex = roughness.Index;
					if(mesh.Material.HeightTexture?.Get().Texture is VkTexture displacement) pc.HeightTextureIndex = displacement.Index;

					vkPlatform.API.CmdPushConstants(
						vkQueue.CommandBuffer,
						((VkRenderPipeline) queue.Pipeline!).PipelineLayout,
						ShaderStageFlags.All,
						0,
						(uint) sizeof(PushConstants),
						&pc
					);

					// this doesn't bind the textures per-se, but rather adds them to the textures descriptor array (if they aren't already)...
					mesh.Material.AlbedoTexture?.Get().Texture?.Bind(queue, 0);
					mesh.Material.NormalTexture?.Get().Texture?.Bind(queue, 0);
					mesh.Material.MetallicTexture?.Get().Texture?.Bind(queue, 0);
					mesh.Material.RoughnessTexture?.Get().Texture?.Bind(queue, 0);
					mesh.Material.HeightTexture?.Get().Texture?.Bind(queue, 0);

					vkPlatform.API.CmdDraw(
						vkQueue.CommandBuffer,
						(uint)mesh.Indices.Length,
						1,
						0,
						0
					);

					instance.VertexOffset += (uint)mesh.Vertices.Length;
					instance.IndexOffset += (uint)mesh.Indices.Length;
					instance.MaterialOffset += 1;
				}

				instance.MatrixOffset += 1;
			}
		}

		public override void Reset() {
			_objects.Clear();
			_matrices.Clear();
			
			while(_pendingAdd.TryDequeue(out _)) { }
			while(_pendingRemove.TryDequeue(out _)) { }
		}

		private record ObjectData(
			RenderableObject Object,
			List<Mesh> Meshes,
			Vertex[] Vertices,
			uint[] Indices,
			Material.Data[] Materials
		);

		private class DrawInstance {
			
			public List<Mesh> Meshes;
			public uint VertexOffset;
			public uint IndexOffset;
			public uint MaterialOffset;
			public uint MatrixOffset;
		
			public DrawInstance(List<Mesh> meshes, uint vo, uint io, uint mo, uint ma) {
				Meshes = meshes;
				VertexOffset = vo;
				IndexOffset = io;
				MaterialOffset = mo;
				MatrixOffset = ma;
			}
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
			[FieldOffset(32)] public int HeightTextureIndex;
		}
	}
}
