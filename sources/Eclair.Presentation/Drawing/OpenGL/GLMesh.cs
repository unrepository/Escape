using System.Runtime.InteropServices;
using Eclair.Renderer;

namespace Eclair.Presentation.Drawing.OpenGL {
	
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GLMesh {

		public Vertex[] Vertices;
		public uint[] Indices;
	}
}
