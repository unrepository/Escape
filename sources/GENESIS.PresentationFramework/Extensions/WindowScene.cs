using System.Diagnostics;
using GENESIS.GPU;

namespace GENESIS.PresentationFramework.Extensions {
	
	public static class WindowScene {

		private static readonly Dictionary<Window, List<Scene>> _windowScenes = [];

		public static void PushScene(this Window window, Scene scene) {
			if(!_windowScenes.ContainsKey(window)) {
				_windowScenes[window] = [];
			}
			
			_windowScenes[window].Add(scene);
			scene.Initialize(window);
			window.RenderQueues[Window.QueuePriority.Normal].Add(scene.Render);
		}

		public static void PopScene(this Window window, Scene scene) {
			Debug.Assert(_windowScenes.ContainsKey(window));
			Debug.Assert(_windowScenes[window].Contains(scene));
			
			_windowScenes[window].Remove(scene);
			window.RenderQueues[Window.QueuePriority.Normal].Remove(scene.Render);
			scene.Deinitialize(window);
		}

		public static Scene? GetSceneById(this Window window, string id) {
			if(!_windowScenes.TryGetValue(window, out var scenes)) {
				return null;
			}

			foreach(var scene in scenes) {
				if(scene.Id == id) {
					return scene;
				}
			}

			return null;
		}
	}
}
