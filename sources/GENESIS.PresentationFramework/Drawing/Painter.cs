using GENESIS.GPU;

namespace GENESIS.PresentationFramework.Drawing {
	
	public struct Painter {
		
		public IPlatform Platform { get; init; }
		public Painter2D XY { get; init; }
		public Painter3D XYZ { get; init; }
	}
}
