using Escape.Renderer;

namespace Escape.Editors.ResourceEditor.FileEditors {
	
	public class TextFileEditor : FileEditor {

		public override string[] FileExtensions => [ ".txt", ".json" ];
		
		public override void Render(RenderQueue queue, TimeSpan delta) {
			Console.WriteLine(File.Name);
		}
	}
}
