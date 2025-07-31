using System.Numerics;
using System.Runtime.InteropServices;

namespace Visio.Renderer.Lights {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct PointLight {

		[FieldOffset(0)] public Color Color;
		[FieldOffset(16)] public Vector3 Position;
		[FieldOffset(28)] private float _padding0;
	}
}
