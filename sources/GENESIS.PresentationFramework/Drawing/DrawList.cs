using System.Numerics;
using System.Runtime.InteropServices;
using GENESIS.GPU;
using GENESIS.GPU.Shader;

namespace GENESIS.PresentationFramework.Drawing {
	
	public abstract class DrawList : IDisposable {

		public List<Vertex[]> Objects = [];
		public List<uint> ObjectIndices = [];
		public List<Vector4> Colors = [];
		public List<Matrix4x4> Matrices = [];

		public Dictionary<string, uint> Models = [];

		public abstract void Render(IShader shader);

		public void RemoveAt(int index) {
			Objects.RemoveAt(index);
			ObjectIndices.RemoveAt(index);
			Colors.RemoveAt(index);
			Matrices.RemoveAt(index);
		}

		public abstract void Dispose();
	}
}
