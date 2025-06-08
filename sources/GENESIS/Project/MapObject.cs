using GENESIS.GPU;
using GENESIS.PresentationFramework.Extensions;
using GENESIS.UI.Object;

namespace GENESIS.Project {
	
	public class MapObject : IProjectObject {

		public FileInfo File { get; }

		public string Ext => ".gmap";
		public static string Extension => ".gmap";

		public string ProjectPath
			=> Path.GetRelativePath(ProjectManager.BaseDirectory.FullName, File.FullName);

		public MapObject(FileInfo file) {
			File = file;
		}
		
		public void Open(IPlatform platform, Window window) {
			window.PushScene(new MapEditor(platform, this));
		}
	}
}
