using System.Runtime.InteropServices;
using Cinenic.Renderer;

namespace Cinenic.Presentation.Drawing.OpenGL {
	
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GLMesh {

		public Vertex[] Vertices;
		public uint[] Indices;
	}
}
