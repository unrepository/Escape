using System.Numerics;
using System.Runtime.InteropServices;

namespace Visio.Renderer {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct Vertex() {
		
		[FieldOffset(0)] public Vector3 Position;
		[FieldOffset(16)] public Vector3 Normal = Vector3.Zero;
		[FieldOffset(32)] public Vector3 Tangent = Vector3.Zero;
		[FieldOffset(48)] public Vector3 Bitangent = Vector3.Zero;
		[FieldOffset(64)] public Vector2 UV = Vector2.Zero;
		[FieldOffset(72)] public Vector2 _padding0 = Vector2.Zero;

		public override string ToString() {
			return $"[Position={Position}, Normal={Normal}, UV={UV}]";
		}
	}
}
