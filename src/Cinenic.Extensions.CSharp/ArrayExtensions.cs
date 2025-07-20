using System.Diagnostics.Contracts;

namespace Cinenic.Extensions.CSharp {
	
	public static class ArrayExtensions {

		[Pure]
		public static T[] Concat<T>(this T[] one, T[] two) {
			var result = new T[one.Length + two.Length];
			one.CopyTo(result, 0);
			two.CopyTo(result, one.Length);

			return result;
		}

		public static void RemoveRange<T>(ref T[] arr, int index, int count) {
			int newLength = arr.Length - count;
			
			Array.Copy(arr, index + count, arr, index, arr.Length - index - count);
			Array.Resize(ref arr, newLength);
		}
	}
}
