using System.Numerics;
using System.Runtime.InteropServices;

namespace Cinenic.Renderer {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct Color {

	#region Known colors
		public static readonly Color White = new(255, 255, 255);
	#endregion
		
		[FieldOffset(0)] public float R;
		[FieldOffset(4)] public float G;
		[FieldOffset(8)] public float B;
		[FieldOffset(12)] public float A;

		public Color(byte r, byte g, byte b, byte a = 255) {
			R = r / 255.0f;
			G = g / 255.0f;
			B = b / 255.0f;
			A = a / 255.0f;
		}

		public Color(float r, float g, float b, float a = 1.0f) {
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public static implicit operator Color(System.Drawing.Color c) => new(c.R, c.G, c.B, c.A);
		public static implicit operator System.Drawing.Color(Color c) => System.Drawing.Color.FromArgb((int) (c.A * 255), (int) (c.R * 255), (int) (c.G * 255), (int) (c.B * 255));
		
		public static implicit operator Vector4(Color c) => new(c.R, c.G, c.B, c.A);
		public static implicit operator Color(Vector4 v) => new(v.X, v.Y, v.Z, v.W);
		
		public static implicit operator Vector3(Color c) => new(c.R, c.G, c.B);
		public static implicit operator Color(Vector3 v) => new(v.X, v.Y, v.Z);
	}
}
