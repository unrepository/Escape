using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using Silk.NET.OpenGL;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	// TODO
	public class GLInstancedDrawList : DrawList {

		internal GLEnum GLShapeType;
		
		private IShaderArrayData<uint> _mappingData;
		private IShaderArrayData<Vertex> _verticesData;
		private IShaderArrayData<uint> _indicesData;
		//private IShaderData<GLMesh> _meshData;
		private IShaderArrayData<Material> _materialsData;
		private IShaderArrayData<Matrix4x4> _matricesData;

		private readonly GLPlatform _platform;
		
		public GLInstancedDrawList(GLPlatform platform, ShapeType type) : base(type, true) {
			_platform = platform;
			
			_mappingData = IShaderArrayData.Create<uint>(platform, 10, null, 0);
			_verticesData = IShaderArrayData.Create<Vertex>(platform, 11, null, 0);
			_indicesData = IShaderArrayData.Create<uint>(platform, 12, null, 0);
			_materialsData = IShaderArrayData.Create<Material>(platform, 13, null, 0);
			_matricesData = IShaderArrayData.Create<Matrix4x4>(platform, 14, null, 0);

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
			/*_mappingData.Size = (uint) InstanceMeshMapping.Count * (uint) sizeof(uint);
			_verticesData.Size = (uint) Vertices.Count * (uint) sizeof(Vertex);
			_indicesData.Size = (uint) Indices.Count * (uint) sizeof(uint);
			_materialsData.Size = (uint) Materials.Count * (uint) sizeof(Material);
			_matricesData.Size = (uint) Matrices.Count * (uint) sizeof(Matrix4x4);

			// TODO test NoCopy
			_mappingData.Data = InstanceMeshMapping.ToArray();
			_verticesData.Data = Vertices.ToArray();
			_indicesData.Data = Indices.ToArray();
			_materialsData.Data = Materials.ToArray();
			_matricesData.Data = Matrices.ToArray();*/
			
			_mappingData.Push();
			_verticesData.Push();
			_indicesData.Push();
			_materialsData.Push();
			_matricesData.Push();
		}

		public override void Draw() {
			throw new NotImplementedException();
		}

		public override void Clear() {
			// foreach(var texture in Textures) {
			// 	_bindlessTexture.MakeTextureHandleNonResident(texture);
			// }
			
			Textures.Clear();
			
			/*InstanceMeshMapping.Clear();
			Vertices.Clear();
			Indices.Clear();*/
			Materials.Clear();
			Matrices.Clear();
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			Clear();
			
			_mappingData.Dispose();
			_verticesData.Dispose();
			_indicesData.Dispose();
			_materialsData.Dispose();
			_matricesData.Dispose();
		}
	}
}
