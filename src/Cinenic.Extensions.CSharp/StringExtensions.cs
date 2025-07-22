namespace Cinenic.Extensions.CSharp {
	
	public static class StringExtensions {
		
		public static string ReplaceLast(this string source, string find, string replace, StringComparison comparison = StringComparison.CurrentCulture) {
			int place = source.LastIndexOf(find, comparison);
			if(place == -1) return source;
    
			return source.Remove(place, find.Length).Insert(place, replace);
		}
	}
}
