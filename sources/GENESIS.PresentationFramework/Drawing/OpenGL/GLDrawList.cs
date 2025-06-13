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
		//private ShaderArrayData<uint> _objectIndicesData = new();
		private IShaderArrayData<Vector4> _colorsData;
		private IShaderArrayData<Matrix4x4> _matricesData;

		public GLDrawList(GLPlatform platform, ShapeType type) : base(type) {
			_verticesData = IShaderArrayData.Create<Vertex>(platform, 10, null, 0);
			_colorsData = IShaderArrayData.Create<Vector4>(platform, 11, null, 0);
			_matricesData = IShaderArrayData.Create<Matrix4x4>(platform, 12, null, 0);

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
			_verticesData.Size = (uint) Vertices.Count * (uint) sizeof(Vertex);
			_colorsData.Size = (uint) Colors.Count * (uint) sizeof(Vector4);
			_matricesData.Size = (uint) Matrices.Count * (uint) sizeof(Matrix4x4);

			_verticesData.Data = Vertices.ToArray();
			_colorsData.Data = Colors.ToArray();
			_matricesData.Data = Matrices.ToArray();
			
			_verticesData.Push();
			_colorsData.Push();
			_matricesData.Push();
		}

		public override void Clear() {
			Vertices.Clear();
			Colors.Clear();
			Matrices.Clear();
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			Clear();
			
			_verticesData.Dispose();
			_colorsData.Dispose();
			_matricesData.Dispose();
		}
	}
}
