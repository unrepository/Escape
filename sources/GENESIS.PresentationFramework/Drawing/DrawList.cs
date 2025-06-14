using System.Numerics;
using System.Runtime.InteropServices;
using GENESIS.GPU;
using GENESIS.GPU.Shader;

namespace GENESIS.PresentationFramework.Drawing {
	
	public abstract class DrawList : IDisposable {

		public bool IsEnabled { get; set; } = true;
		public bool IsInstanced { get; } = false;

		public List<Mesh> Meshes { get; } = [];
		public List<Material> Materials { get; } = [];
		public List<Texture?[]> Textures { get; } = [];
		public List<Matrix4x4> Matrices { get; } = [];

		public ShapeType Type { get; }

		public DrawList(ShapeType type, bool instanced) {
			Type = type;
			IsInstanced = instanced;
		}

		public abstract void Push();
		public abstract void Draw();
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
