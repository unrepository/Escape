using System.Diagnostics;

namespace GENESIS.Project {
	
	public static class ProjectManager {
		
		public static DirectoryInfo BaseDirectory { get; private set; }

		public static void Load(string path) {
			BaseDirectory = new DirectoryInfo(path);
			Debug.Assert(BaseDirectory.Exists);
		}
	}
}
