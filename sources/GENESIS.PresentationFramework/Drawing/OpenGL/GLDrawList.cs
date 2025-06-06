using System.Numerics;
using GENESIS.GPU;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	public class GLDrawList : DrawList {

		private ShaderArrayData<Vertex[]> _objectsData = new();
		private ShaderArrayData<uint> _objectIndicesData = new();
		private ShaderArrayData<Vector4> _colorsData = new();
		private ShaderArrayData<Matrix4x4> _matricesData = new();
		
		public override void Render(IShader shader) {
			_objectsData.Data = Objects.ToArrayNoCopy();
			_objectIndicesData.Data = ObjectIndices.ToArrayNoCopy();
			_colorsData.Data = Colors.ToArrayNoCopy();
			_matricesData.Data = Matrices.ToArrayNoCopy();
			
			// TODO need to resize too :(
			
			shader.PushData(_objectsData);
			shader.PushData(_objectIndicesData);
			shader.PushData(_colorsData);
			shader.PushData(_matricesData);
		}

		public override void Dispose() {
			GC.SuppressFinalize(this);
			_objectsData.Dispose();
			_objectIndicesData.Dispose();
			_colorsData.Dispose();
			_matricesData.Dispose();
		}
	}
}
