using System.Numerics;
using GENESIS.GPU;

namespace GENESIS.PresentationFramework.Drawing {
	
	public static class Models {

		public static readonly Vertex[] Cube = [
			new Vertex { Position = new Vector3(-1, -1, -1) }, // 0
			new Vertex { Position = new Vector3(1, -1, -1) },  // 1
			new Vertex { Position = new Vector3(1, 1, -1) },   // 2
			new Vertex { Position = new Vector3(-1, 1, -1) },  // 3
			new Vertex { Position = new Vector3(-1, -1, 1) },  // 4
			new Vertex { Position = new Vector3(1, -1, 1) },   // 5
			new Vertex { Position = new Vector3(1, 1, 1) },    // 6
			new Vertex { Position = new Vector3(-1, 1, 1) },   // 7
		];
	}
}
