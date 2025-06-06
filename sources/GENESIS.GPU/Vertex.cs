using System.Numerics;
using System.Runtime.InteropServices;

namespace GENESIS.GPU {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct Vertex() {
		
		[FieldOffset(0)] public Vector3 Position;
		//[FieldOffset(16)] public Vector4 Color = Vector4.One;
		[FieldOffset(16)] public Vector3 Normal = Vector3.Zero;
		[FieldOffset(28)] private float _padding0 = 0;
	}
}
