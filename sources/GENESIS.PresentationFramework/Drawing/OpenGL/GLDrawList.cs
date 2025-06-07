using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	public class GLDrawList : DrawList {

		private ShaderArrayData<Vertex> _verticesData = new() { Binding = 10 };
		//private ShaderArrayData<uint> _objectIndicesData = new();
		private ShaderArrayData<Vector4> _colorsData = new() { Binding = 11 };
		private ShaderArrayData<Matrix4x4> _matricesData = new() { Binding = 12 };
		
		public unsafe override void Push(IShader shader) {
			_verticesData.Size = (uint) Vertices.Count * (uint) sizeof(Vertex);
			_colorsData.Size = (uint) Colors.Count * (uint) sizeof(Vector4);
			_matricesData.Size = (uint) Matrices.Count * (uint) sizeof(Matrix4x4);

			_verticesData.Data = Vertices.ToArray();
			_colorsData.Data = Colors.ToArray();
			_matricesData.Data = Matrices.ToArray();
			
			if(_verticesData.Owner is null) shader.PushData(_verticesData);
			if(_colorsData.Owner is null) shader.PushData(_colorsData);
			if(_matricesData.Owner is null) shader.PushData(_matricesData);
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
