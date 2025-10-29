using System.Diagnostics;
using System.Reflection;
using Escape.Renderer;
using Escape.Resources;
using NLog;

namespace Escape.Editor {
	
	public static class ProjectResources {

		public static Dictionary<FileInfo, dynamic> AllResources { get; } = [];

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		public static void Load(IPlatform platform, DirectoryInfo assetsDirectory) {
			Debug.Assert(ProjectGlobals.ProjectInfo is not null);
			Debug.Assert(ProjectGlobals.OutputDirectory is not null);
			
			AllResources.Clear();

			var resourceLoad = typeof(ResourceManager).GetMethod(
				"Load",
				BindingFlags.Public | BindingFlags.Static
			);
			
			Debug.Assert(resourceLoad is not null);

			//foreach(var @namespace in assetsDirectory.EnumerateDirectories()) {
			//	var assembly = Assembly.LoadFile(Path.Combine(ProjectGlobals.OutputDirectory.FullName, @namespace.Name, ".dll"));

			var assembly = ProjectGlobals.ProjectAssembly!;
			
				foreach(var file in assetsDirectory.EnumerateFiles("*", SearchOption.AllDirectories)) {
					if(file.Name.EndsWith(".meta.json")) continue;
					if(file.Name.StartsWith('.')) continue;

					var extension = file.Extension;
					var format = ResourceRegistry.GetFormatByExtension(extension);

					if(format is null) {
						_logger.Warn(
							"{ProjectName}: Could not get a format for resource {Resource}",
							ProjectGlobals.ProjectInfo.Name, file.FullName
						);
						
						continue;
					}
					
					dynamic? resource =
						resourceLoad
							.MakeGenericMethod(format.Value.ResourceType)
							.Invoke(null, [ platform, file.FullName, false, true, assembly ]);

					if(resource is null) {
						_logger.Warn(
							"{ProjectName}: Failed to load resource {Resource}",
							ProjectGlobals.ProjectInfo.Name, file.FullName
						);
						
						continue;
					}

					AllResources[file] = resource;
				}
			//}
		}
	}
}
