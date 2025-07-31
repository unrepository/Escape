using System.Numerics;
using System.Runtime.InteropServices;

namespace Visio.Renderer.Camera {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct CameraData {

		[FieldOffset(0)] public Matrix4x4 Projection;
		[FieldOffset(64)] public Matrix4x4 InverseProjection;
		[FieldOffset(128)] public Matrix4x4 View;
		[FieldOffset(192)] public Matrix4x4 InverseView;
		[FieldOffset(256)] public Vector3 Position;
		[FieldOffset(268)] public float AspectRatio;
	}
}
