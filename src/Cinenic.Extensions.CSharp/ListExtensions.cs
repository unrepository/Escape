using System.Reflection;

namespace Cinenic.Extensions.CSharp {
	
	public static class ListExtensions {
		
		public static T[] ToArrayNoCopy<T>(this List<T> list, bool clear = false) {
			var field = typeof(List<T>).GetField("_items",
				BindingFlags.Instance | BindingFlags.NonPublic);

			if(field == null) {
				throw new MissingFieldException("Cannot find the _items field.");
			}

			if(clear) list.Clear();

			return (T[]) field.GetValue(list);
		}
	}
}
