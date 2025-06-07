using System.Numerics;
using GENESIS.GPU;

namespace GENESIS.PresentationFramework.Drawing {
	
	public static class Models {

		public const int CUBE_ID = 0;
		public static readonly Vertex[] Cube = [
			    new Vertex { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3(-1.0f, -1.0f,  1.0f) },
			    new Vertex { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3( 1.0f,  1.0f, -1.0f) },
			    new Vertex { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3(-1.0f,  1.0f, -1.0f) },
			    new Vertex { Position = new Vector3( 1.0f, -1.0f,  1.0f) },
			    new Vertex { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3( 1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3( 1.0f,  1.0f, -1.0f) },
			    new Vertex { Position = new Vector3( 1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3(-1.0f,  1.0f, -1.0f) },
			    new Vertex { Position = new Vector3( 1.0f, -1.0f,  1.0f) },
			    new Vertex { Position = new Vector3(-1.0f, -1.0f,  1.0f) },
			    new Vertex { Position = new Vector3(-1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3(-1.0f, -1.0f,  1.0f) },
			    new Vertex { Position = new Vector3( 1.0f, -1.0f,  1.0f) },
			    new Vertex { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3( 1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3( 1.0f,  1.0f, -1.0f) },
			    new Vertex { Position = new Vector3( 1.0f, -1.0f, -1.0f) },
			    new Vertex { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3( 1.0f, -1.0f,  1.0f) },
			    new Vertex { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3( 1.0f,  1.0f, -1.0f) },
			    new Vertex { Position = new Vector3(-1.0f,  1.0f, -1.0f) },
			    new Vertex { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3(-1.0f,  1.0f, -1.0f) },
			    new Vertex { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3( 1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3(-1.0f,  1.0f,  1.0f) },
			    new Vertex { Position = new Vector3( 1.0f, -1.0f,  1.0f) }
		];
	}
}
