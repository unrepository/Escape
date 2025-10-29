using Escape.Renderer;
using Escape.Resources;

namespace Escape.Editors.ResourceEditor.FileEditors {
	
	public abstract class FileEditor : IRenderer {
		
		public string Id { get; }
		public int Priority { get; init; }
		
		public abstract string[] FileExtensions { get; }
		
		public FileInfo File { get; }
		
		public FileEditor() { }

		public FileEditor(FileInfo file) {
			Id = file.Name;
			File = file;
		}
		
		public abstract void Render(RenderQueue queue, TimeSpan delta);
	}
}
