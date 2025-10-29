using System.Reflection;

namespace Escape.Editor {
	
	public static class ProjectGlobals {
		
		public static ProjectInfo? ProjectInfo { get; set; }
		public static DirectoryInfo? ProjectDirectory { get; set; }

		public static DirectoryInfo? ResourcesDirectory
			=> ProjectInfo is null || ProjectDirectory is null
				? null
				: new DirectoryInfo(Path.Combine(ProjectDirectory.FullName, ProjectInfo.ResourcesDirectory));
		
		public static DirectoryInfo? OutputDirectory
			=> ProjectInfo is null || ProjectDirectory is null
				? null
				: new DirectoryInfo(Path.Combine(ProjectDirectory.FullName, ProjectInfo.OutputDirectory));
		
		public static Assembly? ProjectAssembly { get; set; }
	}
}
