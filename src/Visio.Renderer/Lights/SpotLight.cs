using System.Numerics;
using System.Runtime.InteropServices;

namespace Visio.Renderer.Lights {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct SpotLight {

		[FieldOffset(0)] public Color Color;
		[FieldOffset(16)] public Vector3 Position;
		[FieldOffset(32)] public Vector3 Direction;
		
		[FieldOffset(48)] public float Cutoff;
		[FieldOffset(52)] public float CutoffOuter;
		
		[FieldOffset(56)] private float _padding0;
		[FieldOffset(60)] private float _padding1;
	}
}
