using Eclair.Renderer;
using Eclair.Presentation.Extensions;
using GENESIS.UI.Object;
using GENESIS.UI.Simulation;

namespace GENESIS.Project {
	
	public class NBodySimulationObject : IProjectObject {

		public FileInfo File { get; }

		public string Ext => ".nbsim";
		public static string Extension => ".nbsim";

		public string ProjectPath
			=> Path.GetRelativePath(ProjectManager.BaseDirectory.FullName, File.FullName);

		public NBodySimulationObject(FileInfo file) {
			File = file;
		}
		
		public void Open(IPlatform platform, Window window) {
			window.PushScene(new NBodySimulationEditor(platform, this));
		}
	}
}
