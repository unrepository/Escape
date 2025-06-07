using System.Diagnostics.Contracts;

namespace GENESIS.LanguageExtensions {
	
	public static class ArrayExtensions {

		[Pure]
		public static T[] Concat<T>(this T[] one, T[] two) {
			var result = new T[one.Length + two.Length];
			one.CopyTo(result, 0);
			two.CopyTo(result, one.Length);

			return result;
		}
	}
}
