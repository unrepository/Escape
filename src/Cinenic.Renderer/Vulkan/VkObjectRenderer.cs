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

		private readonly Dictionary<RenderableObject, ObjectDrawData> _drawData = [];
		//private readonly List<RenderableObject> _objects = [];

		public VkObjectRenderer(string id, DefaultSceneShaderPipeline shaderPipeline) : base(id, shaderPipeline) { }

		public unsafe override bool AddObject(RenderableObject obj, Matrix4x4? matrix = null) {
			matrix ??= Matrix4x4.Identity;

			var model = obj.Model;
			var vertices = model.Meshes.SelectMany(m => m.Vertices).ToArray();
			var indices = model.Meshes.SelectMany(m => m.Indices).ToArray();
			var materials = model.Meshes.Select(m => m.Material.CreateData()).ToArray();

			var lastData = _drawData.Values.LastOrDefault(new ObjectDrawData(0, 0, 0, 0, 0, 0, -1));
			var data = new ObjectDrawData(
				lastData.VertexIndex + lastData.VertexCount,
				(uint) vertices.Length,
				lastData.IndexIndex + lastData.IndexCount,
				(uint) indices.Length,
				lastData.MaterialIndex + lastData.MaterialCount,
				(uint) materials.Length,
				lastData.MatrixIndex + 1
			);

			_drawData[obj] = data;
			
			ShaderPipeline.VertexData.Write(data.VertexIndex, vertices);
			ShaderPipeline.IndexData.Write(data.IndexIndex, indices);
			ShaderPipeline.MaterialData.Write(data.MaterialIndex, materials);
			ShaderPipeline.MatrixData.Write((uint) data.MatrixIndex, [ matrix.Value ]);

			_logger.Trace("Added object {Id}", obj.Id);
			return true;
		}

		public unsafe override bool SetMatrix(RenderableObject obj, Matrix4x4 matrix) {
			ShaderPipeline.MatrixData.Write((uint) _drawData[obj].MatrixIndex, [ matrix ]);
			return true;
		}

		// TODO broken under stupid circumstances
		public override bool RemoveObject(RenderableObject obj) {
			if(!_drawData.TryGetValue(obj, out var data)) return false;

			if(!ShaderPipeline.VertexData.Remove(data.VertexIndex, data.VertexCount)) return false;
			if(!ShaderPipeline.IndexData.Remove(data.IndexIndex, data.IndexCount)) return false;
			if(!ShaderPipeline.MaterialData.Remove(data.MaterialIndex, data.MaterialCount)) return false;
			if(!ShaderPipeline.MatrixData.Remove((uint) data.MatrixIndex, 1)) return false;

			data.VertexCount = 0;
			_drawData[obj] = data;
			//return _drawData.Remove(obj);
			return true;
		}

		public unsafe override void Render(RenderQueue queue, TimeSpan delta) {
			Debug.Assert(queue is VkRenderQueue);
			var vkQueue = (VkRenderQueue) queue;
			var vkPlatform = (VkPlatform) queue.Platform;

			ShaderPipeline.PushData();

			uint vo = 0;
			uint io = 0;
			uint mo = 0;
			uint mxo = 0;

			foreach(var (obj, data) in _drawData) {
				if(data.VertexCount == 0 || data.IndexCount == 0) {
					mxo += 1;
					continue;
				}
				
				foreach(var mesh in obj.Model.Meshes) {
					var pc = new PushConstants {
						VertexOffset = vo,
						IndexOffset = io,
						MaterialOffset = mo,
						MatrixOffset = mxo
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
						(uint) mesh.Indices.Length,
						1,
						0,
						0
					);

					vo += (uint) mesh.Vertices.Length;
					io += (uint) mesh.Indices.Length;
					mo += 1;
				}

				mxo += 1;
			}
		}

		public override void Reset() {
			_drawData.Clear();
		}

		private record struct ObjectDrawData(
			uint VertexIndex,
			uint VertexCount,
			uint IndexIndex,
			uint IndexCount,
			uint MaterialIndex,
			uint MaterialCount,
			int MatrixIndex
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
