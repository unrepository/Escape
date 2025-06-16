using System.Numerics;
using System.Runtime.InteropServices;

namespace Eclair.Renderer {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct Vertex() {
		
		[FieldOffset(0)] public Vector3 Position;
		[FieldOffset(16)] public Vector3 Normal = Vector3.Zero;
		[FieldOffset(32)] public Vector2 UV = Vector2.Zero;
		[FieldOffset(40)] public Vector2 _padding0 = Vector2.Zero;

		public override string ToString() {
			return $"[Position={Position}, Normal={Normal}, UV={UV}]";
		}
	}
}
