using System.Numerics;
using System.Runtime.InteropServices;
using GENESIS.GPU;
using GENESIS.GPU.Shader;

namespace GENESIS.PresentationFramework.Drawing {
	
	public abstract class DrawList : IDisposable {

		public bool Enabled { get; set; } = true;

		public List<Vertex> Vertices = [];
		//public List<uint> ObjectIndices = [];
		public List<Vector4> Colors = [];
		public List<Matrix4x4> Matrices = [];

		public ShapeType Type { get; }
		
		//public Dictionary<string, Vertex[]> Models = [];
		public string? Model = null;

		public DrawList(ShapeType type) {
			Type = type;
		}

		public abstract void Push();
		public abstract void Clear();
		
		public abstract void Dispose();

		public enum ShapeType {
			
			Triangle,
			TriangleStrip,
			TriangleFan,
			Line,
			LineStrip,
			Point
		}
	}
}
