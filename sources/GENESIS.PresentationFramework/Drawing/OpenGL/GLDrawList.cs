using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using Silk.NET.OpenGL;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	public class GLDrawList : DrawList {

		internal GLEnum GLShapeType;
		
		private IShaderArrayData<Vertex> _verticesData;
		private IShaderArrayData<uint> _indicesData;
		private IShaderData<Material> _materialData;
		private IShaderData<Matrix4x4> _matrixData;

		private readonly GLPlatform _platform;
		
		public unsafe GLDrawList(GLPlatform platform, ShapeType type) : base(type, false) {
			_platform = platform;
			
			_verticesData = IShaderArrayData.Create<Vertex>(platform, 11, null, 0);
			_indicesData = IShaderArrayData.Create<uint>(platform, 12, null, 0);
			_materialData = IShaderData.Create<Material>(platform, 13, new() /* TODO why can't this be null??? */, (uint) sizeof(Material));
			_matrixData = IShaderData.Create<Matrix4x4>(platform, 14, new(), (uint) sizeof(Matrix4x4));
			
			GLShapeType = type switch {
				ShapeType.Triangle => GLEnum.Triangles,
				ShapeType.TriangleStrip => GLEnum.TriangleStrip,
				ShapeType.TriangleFan => GLEnum.TriangleFan,
				ShapeType.Line => GLEnum.Line,
				ShapeType.LineStrip => GLEnum.LineStrip,
				ShapeType.Point => GLEnum.Points,
				_ => throw new NotImplementedException()
			};
		}
		
		public unsafe override void Push() {
			
		}

		public unsafe override void Draw() {
			foreach(var (i, mesh) in Meshes.Enumerate()) {
				_verticesData.Size = (uint) mesh.Vertices.Length * (uint) sizeof(Vertex);
				_indicesData.Size = (uint) mesh.Indices.Length * (uint) sizeof(uint);

				_verticesData.Data = mesh.Vertices;
				_indicesData.Data = mesh.Indices;
				_materialData.Data = Materials[i];
				_matrixData.Data = Matrices[i];
				
				_verticesData.Push();
				_indicesData.Push();
				_materialData.Push();
				_matrixData.Push();
				
				foreach(var texture in Textures[i]) {
					texture?.Bind();
				}
				
				_platform.API.DrawArrays(
					GLShapeType,
					0,
					(uint) mesh.Indices.Length
				);
			}
		}

		public override void Clear() {
			// foreach(var texture in Textures) {
			// 	_bindlessTexture.MakeTextureHandleNonResident(texture);
			// }
			
			Meshes.Clear();
			Textures.Clear();
			Matrices.Clear();
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			Clear();
			
			_verticesData.Dispose();
			_indicesData.Dispose();
			_materialData.Dispose();
			_matrixData.Dispose();
		}
	}
}
