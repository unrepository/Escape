namespace Escape.Extensions.CSharp {
	
	public static class PathExtensions {

		public static string GetRealPath(string path) {
			path = Path.GetFullPath(path);
			
			var components = path.Split(Path.DirectorySeparatorChar);
			var realPath = components[0];

			if(components.Length <= 1) return path;
			if(string.IsNullOrWhiteSpace(realPath)) realPath = Path.DirectorySeparatorChar.ToString();

			for(int i = 1; i < components.Length; i++) {
				var subPath = Path.Combine(realPath, components[i]);
				var link = Directory.ResolveLinkTarget(subPath, true);

				if(link is null) {
					realPath = subPath;
					continue;
				}

				realPath = link.FullName;
			}

			return realPath;
		}
	}
}
