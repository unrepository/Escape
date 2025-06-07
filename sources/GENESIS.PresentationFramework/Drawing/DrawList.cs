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

		public Dictionary<string, Vertex[]> Models = [];
		public string? Model = null;

		public abstract void Push(Shader shader);
		public abstract void Clear();
		
		public abstract void Dispose();
	}
}
