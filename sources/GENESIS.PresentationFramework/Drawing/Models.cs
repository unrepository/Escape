using System.Numerics;
using GENESIS.GPU;

namespace GENESIS.PresentationFramework.Drawing {
	
	public static class Models {

		public static readonly Vertex[] Cube = [
		    new() { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3(-1.0f, -1.0f,  1.0f) },
		    new() { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3( 1.0f,  1.0f, -1.0f) },
		    new() { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3(-1.0f,  1.0f, -1.0f) },
		    new() { Position = new Vector3( 1.0f, -1.0f,  1.0f) },
		    new() { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3( 1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3( 1.0f,  1.0f, -1.0f) },
		    new() { Position = new Vector3( 1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3(-1.0f,  1.0f, -1.0f) },
		    new() { Position = new Vector3( 1.0f, -1.0f,  1.0f) },
		    new() { Position = new Vector3(-1.0f, -1.0f,  1.0f) },
		    new() { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3(-1.0f, -1.0f,  1.0f) },
		    new() { Position = new Vector3( 1.0f, -1.0f,  1.0f) },
		    new() { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3( 1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3( 1.0f,  1.0f, -1.0f) },
		    new() { Position = new Vector3( 1.0f, -1.0f, -1.0f) },
		    new() { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3( 1.0f, -1.0f,  1.0f) },
		    new() { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3( 1.0f,  1.0f, -1.0f) },
		    new() { Position = new Vector3(-1.0f,  1.0f, -1.0f) },
		    new() { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3(-1.0f,  1.0f, -1.0f) },
		    new() { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
		    new() { Position = new Vector3( 1.0f, -1.0f,  1.0f) }
		];

		public static readonly Vertex[] Quad = [
			new() { Position = new Vector3(-1, -1, 0) },
			new() { Position = new Vector3(1, -1, 0) },
			new() { Position = new Vector3(-1, 1, 0) },
			new() { Position = new Vector3(-1, 1, 0) },
			new() { Position = new Vector3(1, -1, 0) },
			new() { Position = new Vector3(1, 1, 0) },
		];
	}
}
