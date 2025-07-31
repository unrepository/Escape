using System.Drawing;
using System.Numerics;

namespace Visio.Extensions.CSharp {
	
	public static class ColorExtensions {

		private static readonly Random RANDOM = new();

		public static Vector4 ToVector4(this Color color) {
			return new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}
		
		public static Vector3 ToVector3(this Color color) {
			return new(color.R / 255f, color.G / 255f, color.B / 255f);
		}

		public static Color Randomize(this Color color) {
			return Color.FromArgb(
				RANDOM.Next(byte.MinValue, byte.MaxValue),
				RANDOM.Next(byte.MinValue, byte.MaxValue),
				RANDOM.Next(byte.MinValue, byte.MaxValue),
				RANDOM.Next(byte.MinValue, byte.MaxValue)
			);
		}
	}
}
