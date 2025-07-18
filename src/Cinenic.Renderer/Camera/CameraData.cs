using System.Numerics;
using System.Runtime.InteropServices;

namespace Cinenic.Renderer.Camera {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct CameraData {

		[FieldOffset(0)] public Matrix4x4 Projection;
		[FieldOffset(64)] public Matrix4x4 View;
		[FieldOffset(128)] public Vector3 Position;
		[FieldOffset(140)] private float _padding0;
	}
}
