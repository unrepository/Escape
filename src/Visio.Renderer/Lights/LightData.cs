using System.Runtime.InteropServices;

namespace Visio.Renderer.Lights {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct LightData {

		[FieldOffset(0)] public uint DirectionalCount;
		[FieldOffset(4)] public uint PointCount;
		[FieldOffset(8)] public uint SpotCount;
		[FieldOffset(12)] private float _padding0;
	}
}
