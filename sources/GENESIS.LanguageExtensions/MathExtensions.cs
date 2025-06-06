namespace GENESIS.LanguageExtensions {
	
	public static class MathExtensions {

		public static double ToRadians(this double degrees) {
			return (Math.PI / 180) * degrees;
		}

		public static float ToRadians(this float degrees) {
			return (MathF.PI / 180) * degrees;
		}
	}
}
