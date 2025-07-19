namespace Cinenic.Extensions.CSharp {
	
	public static class MathExtensions {

		public static double ToRadians(this double degrees) {
			return (Math.PI / 180) * degrees;
		}

		public static float ToRadians(this float degrees) {
			return (MathF.PI / 180) * degrees;
		}
		
		public static double ToDegrees(this double radians) {
			return radians * (180 / Math.PI);
		}

		public static float ToDegrees(this float radians) {
			return radians * (180 / MathF.PI);
		}

		public static uint CeilIncrement(this uint num, uint increment) {
			return (num + increment - 1) / increment * increment;
		}
	}
}
