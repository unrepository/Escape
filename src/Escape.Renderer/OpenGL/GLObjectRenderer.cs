using System.Diagnostics;
using System.Numerics;
using Escape.Renderer.Shader.Pipelines;
using NLog;
using Silk.NET.OpenGL;

namespace Escape.Renderer.OpenGL {
	
	public class GLObjectRenderer : ObjectRenderer {
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly GLPlatform _platform;
		private readonly Dictionary<RenderableObject, ObjectDrawData> _objectData = [];

		public GLObjectRenderer(string id, DefaultSceneShaderPipeline shaderPipeline) : base(id, shaderPipeline) {
			_platform = (GLPlatform) shaderPipeline.Platform;
		}

		public unsafe override bool AddObject(RenderableObject obj, Matrix4x4? matrix = null) {
			if(_objectData.TryGetValue(obj, out var drawData)) {
				drawData.Dispose(_platform);
			}
			
			var meshes = obj.Model.Meshes;
			var vaos = new uint[meshes.Count];
			var vbos = new uint[meshes.Count * 2];
			
			fixed(uint* vaoPtr = vaos) {
				_platform.API.GenVertexArrays((uint) vaos.Length, vaoPtr);
			}

			fixed(uint* vboPtr = vbos) {
				_platform.API.GenBuffers((uint) vbos.Length, vboPtr);
			}

			for(int i = 0; i < meshes.Count; i++) {
				var mesh = meshes[i];
				var vao = vaos[i];
				
				_platform.API.BindVertexArray(vao);

			#region vertices & indices
				var vbo = vbos[2 * i];
				var ebo = vbos[2 * i + 1];
				
				_platform.API.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

				fixed(void* dataPtr = mesh.Vertices) {
					_platform.API.BufferData(
						BufferTargetARB.ArrayBuffer,
						(nuint) (mesh.Vertices.Length * sizeof(Vertex)),
						dataPtr,
						BufferUsageARB.StaticDraw
					);
				}
				
				_platform.API.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

				fixed(void* dataPtr = mesh.Indices) {
					_platform.API.BufferData(
						BufferTargetARB.ArrayBuffer,
						(nuint) (mesh.Indices.Length * sizeof(uint)),
						dataPtr,
						BufferUsageARB.StaticDraw
					);
				}
			#endregion

			#region vertex data
				var type = VertexAttribPointerType.Float;
				var size = (uint) sizeof(Vertex);
				
				// vec3 position
				_platform.API.EnableVertexAttribArray(0);
				_platform.API.VertexAttribPointer(0, 3, type, false, size, 0);
				
				// vec3 normal
				_platform.API.EnableVertexAttribArray(1);
				_platform.API.VertexAttribPointer(1, 3, type, false, size, 16);
				
				// vec3 tangent
				_platform.API.EnableVertexAttribArray(2);
				_platform.API.VertexAttribPointer(2, 3, type, false, size, 32);
				
				// vec3 bitangent
				_platform.API.EnableVertexAttribArray(3);
				_platform.API.VertexAttribPointer(3, 3, type, false, size, 48);
				
				// vec2 uv
				_platform.API.EnableVertexAttribArray(4);
				_platform.API.VertexAttribPointer(4, 2, type, false, size, 64);
			#endregion
			}

			_objectData[obj] = new ObjectDrawData {
				Matrix = matrix.GetValueOrDefault(Matrix4x4.Identity),
				VAOs = vaos,
				VBOs = vbos
			};
			
			return true;
		}

		public override bool SetMatrix(RenderableObject obj, Matrix4x4 matrix) {
			if(!_objectData.TryGetValue(obj, out var drawData)) return false;
			drawData.Matrix = matrix;
			return true;
		}

		public override bool RemoveObject(RenderableObject obj) {
			if(!_objectData.TryGetValue(obj, out var drawData)) return false;
			drawData.Dispose(_platform);
			_objectData.Remove(obj);
			return true;
		}
		
		// can't do programmable vertex pulling if we want OpenGL <4.3 support (PVP requires SSBOs)
		public unsafe override void Render(RenderQueue queue, TimeSpan delta) {
			ShaderPipeline.PushData();

			foreach(var (obj, data) in _objectData) {
				for(int i = 0; i < obj.Model.Meshes.Count; i++) {
					var mesh = obj.Model.Meshes[i];
					
					mesh.Material.AlbedoTexture?.Get().Texture?.Bind(queue, 0);
					mesh.Material.NormalTexture?.Get().Texture?.Bind(queue, 1);
					mesh.Material.MetallicTexture?.Get().Texture?.Bind(queue, 2);
					mesh.Material.RoughnessTexture?.Get().Texture?.Bind(queue, 3);
					mesh.Material.HeightTexture?.Get().Texture?.Bind(queue, 4);

					// set model matrix uniform
					var matrix = data.Matrix;
					_platform.API.UniformMatrix4(ShaderPipeline.GLModelMatrixUniform, 1, false, (float*) &matrix);
					
					var vao = data.VAOs[i];
					_platform.API.BindVertexArray(vao);
					
					_platform.API.DrawElements(
						PrimitiveType.Triangles,
						(uint) mesh.Indices.Length,
						DrawElementsType.UnsignedInt,
						null
					);
				}
			}
		}
		
		public override void Reset() {
			foreach(var drawData in _objectData) {
				drawData.Value.Dispose(_platform);
			}
			
			_objectData.Clear();
		}

		private class ObjectDrawData {

			public Matrix4x4 Matrix { get; set; }
			public uint[] VBOs { get; set; }
			public uint[] VAOs { get; set; }

			public unsafe void Dispose(GLPlatform platform) {
				fixed(uint* vboPtr = VBOs) {
					platform.API.DeleteBuffers((uint) VBOs.Length, vboPtr);
				}
				
				fixed(uint* vaoPtr = VAOs) {
					platform.API.DeleteVertexArrays((uint) VAOs.Length, vaoPtr);
				}
			}
		}
	}
}
