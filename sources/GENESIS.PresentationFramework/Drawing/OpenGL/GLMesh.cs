using System.Runtime.InteropServices;
using GENESIS.GPU;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GLMesh {

		public Vertex[] Vertices;
		public uint[] Indices;
	}
}
