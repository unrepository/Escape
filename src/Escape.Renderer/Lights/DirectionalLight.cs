using System.Numerics;
using System.Runtime.InteropServices;

namespace Escape.Renderer.Lights {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct DirectionalLight {

		[FieldOffset(0)] public Color Color;
		[FieldOffset(16)] public Vector3 Direction;
		[FieldOffset(28)] private float _padding0;
	}
}
