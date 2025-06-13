using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ARB;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	public class GLDrawList : DrawList {

		internal GLEnum GLShapeType;
		
		private IShaderArrayData<Vertex> _verticesData;
		private IShaderArrayData<Material> _materialsData;
		private IShaderArrayData<Matrix4x4> _matricesData;

		private readonly GLPlatform _platform;
		private readonly ArbBindlessTexture _bindlessTexture;
		
		public GLDrawList(GLPlatform platform, ShapeType type) : base(type) {
			_platform = platform;
			_bindlessTexture = new(_platform.API.Context);
			
			_verticesData = IShaderArrayData.Create<Vertex>(platform, 10, null, 0);
			_materialsData = IShaderArrayData.Create<Material>(platform, 11, null, 0);
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
			// foreach(var texture in Textures) {
			// 	_bindlessTexture.MakeTextureHandleResident(texture);
			// }
			
			_verticesData.Size = (uint) Vertices.Count * (uint) sizeof(Vertex);
			_materialsData.Size = (uint) Materials.Count * (uint) sizeof(Material);
			_matricesData.Size = (uint) Matrices.Count * (uint) sizeof(Matrix4x4);

			// TODO test NoCopy
			_verticesData.Data = Vertices.ToArray();
			_materialsData.Data = Materials.ToArray();
			_matricesData.Data = Matrices.ToArray();
			
			_verticesData.Push();
			_materialsData.Push();
			_matricesData.Push();
		}

		public override void Clear() {
			// foreach(var texture in Textures) {
			// 	_bindlessTexture.MakeTextureHandleNonResident(texture);
			// }
			
			Textures.Clear();
			Vertices.Clear();
			Materials.Clear();
			Matrices.Clear();
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			Clear();
			
			_verticesData.Dispose();
			_materialsData.Dispose();
			_matricesData.Dispose();
		}
	}
}
