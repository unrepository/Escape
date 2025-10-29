using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Editors.ResourceEditor.FileEditors {
	
	public static class FileEditorRegistry {

		public static Dictionary<string, ConstructorInfo> Editors { get; } = [];

		static FileEditorRegistry() {
			
		}

		public static IRenderer? CreateEditorFor(FileInfo file) {
			var fileExtension = Path.GetExtension(file.Extension);
			
			if(!Editors.TryGetValue(fileExtension, out var editorCtor)) {
				return null;
			}

			var editor = editorCtor.Invoke([ file ]);
			Debug.Assert(editor is IRenderer);

			return (IRenderer) editor;
		}

		public static void Register<TFileEditor>() where TFileEditor : FileEditor, new() {
			var fileExtensions = new TFileEditor().FileExtensions;
			
			var editorCtor = typeof(TFileEditor).GetConstructor([ typeof(FileInfo) ]);
			Debug.Assert(editorCtor is not null);

			foreach(var fileExtension in fileExtensions) {
				Editors[fileExtension] = editorCtor;
			}
		}
	}
}
