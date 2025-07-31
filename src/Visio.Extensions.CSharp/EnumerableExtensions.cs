namespace Visio.Extensions.CSharp {
	
	public static class EnumerableExtensions {

		public static IEnumerable<(int index, T item)> Enumerate<T>(this IEnumerable<T> source) {
			int i = 0;

			foreach(var item in source) {
				yield return (i++, item);
			}
		}
	}
}
