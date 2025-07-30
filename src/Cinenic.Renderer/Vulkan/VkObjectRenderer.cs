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
	
	public class VkObjectRenderer : ObjectRenderer {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly Dictionary<RenderableObject, ObjectDrawData> _drawDatas = [];

		public VkObjectRenderer(string id, DefaultSceneShaderPipeline shaderPipeline) : base(id, shaderPipeline) { }

		public unsafe override bool AddObject(RenderableObject obj, Matrix4x4? matrix = null) {
			matrix ??= Matrix4x4.Identity;

			var model = obj.Model;
			var vertices = model.Meshes.SelectMany(m => m.Vertices).ToArray();
			var indices = model.Meshes.SelectMany(m => m.Indices).ToArray();
			var materials = model.Meshes.Select(m => m.Material.CreateData()).ToArray();

			var lastData = _drawDatas.Values.LastOrDefault();
			var data = new ObjectDrawData(
				obj,
				lastData.VertexOffset + lastData.VertexCount,
				(uint) vertices.Length,
				lastData.IndexOffset + lastData.IndexCount,
				(uint) indices.Length,
				lastData.MaterialOffset + lastData.MaterialCount,
				(uint) materials.Length,
				lastData.MatrixOffset + 1
			);

			_drawDatas[obj] = data;
			
			//lock(ShaderPipeline) {
				// ShaderPipeline.VertexData.Size += (uint) (vertices.Length * sizeof(Vertex));
				// ShaderPipeline.IndexData.Size += (uint) (indices.Length * sizeof(uint));
				// ShaderPipeline.MaterialData.Size += (uint) (materials.Length * sizeof(Material.Data));
				// ShaderPipeline.MatrixData.Size += (uint) (1 * sizeof(Matrix4x4));
			//}
			
			ShaderPipeline.VertexData.Write(data.VertexOffset, vertices);
			ShaderPipeline.IndexData.Write(data.IndexOffset, indices);
			ShaderPipeline.MaterialData.Write(data.MaterialOffset, materials);
			ShaderPipeline.MatrixData.Write(data.MatrixOffset, [ matrix.Value ]);

			_logger.Trace("Added object {Id}", obj.Id);
			return true;
		}

		public unsafe override bool SetMatrix(RenderableObject obj, Matrix4x4 matrix) {
			ShaderPipeline.MatrixData.Write(_drawDatas[obj].MatrixOffset, [ matrix ]);
			return true;
		}

		public override bool RemoveObject(RenderableObject obj) {
			throw new NotImplementedException();
		}

		public unsafe override void Render(RenderQueue queue, TimeSpan delta) {
			Debug.Assert(queue is VkRenderQueue);
			var vkQueue = (VkRenderQueue) queue;
			var vkPlatform = (VkPlatform) queue.Platform;

			ShaderPipeline.PushData();

			/*var barrier = new MemoryBarrier {
				SType = StructureType.MemoryBarrier,
				SrcAccessMask =	AccessFlags.HostWriteBit,
				DstAccessMask = AccessFlags.ShaderReadBit
			};
			
			vkPlatform.API.CmdPipelineBarrier(
				vkQueue.CommandBuffer,
				PipelineStageFlags.HostBit,
				PipelineStageFlags.VertexShaderBit | PipelineStageFlags.FragmentShaderBit,
				0,
				1,
				&barrier,
				0,
				null,
				0,
				null
			);*/

			foreach(var (obj, data) in _drawDatas) {
				var vo = data.VertexOffset;
				var io = data.IndexOffset;
				var mo = data.MaterialOffset;
				
				foreach(var mesh in obj.Model.Meshes) {
					var pc = new PushConstants {
						VertexOffset = vo,
						IndexOffset = io,
						MaterialOffset = mo,
						MatrixOffset = data.MatrixOffset
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

					vo += (uint)mesh.Vertices.Length;
					io += (uint)mesh.Indices.Length;
					mo += 1;
				}
			}
		}

		public override void Reset() {
			_drawDatas.Clear();
		}

		private record struct ObjectDrawData(
			RenderableObject Renderable,
			uint VertexOffset,
			uint VertexCount,
			uint IndexOffset,
			uint IndexCount,
			uint MaterialOffset,
			uint MaterialCount,
			uint MatrixOffset
		);

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
