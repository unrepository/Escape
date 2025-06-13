using System.Numerics;
using System.Runtime.InteropServices;

namespace GENESIS.GPU {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct Material() {
		
		[FieldOffset(0)] public Vector4 Albedo = Vector4.One;
		[FieldOffset(16)] public int HasTextures = 0;
		[FieldOffset(20)] public ulong DiffuseTexture = 0;
		[FieldOffset(28)] private float _padding0 = 0;

		public override string ToString() {
			return $"[Albedo={Albedo}]";
		}
	}
}
