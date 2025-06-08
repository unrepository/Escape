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

			// only one environment scene allowed per window
			if(scene is EnvironmentScene) {
				foreach(var s in window.GetScenes().Where(s => s is EnvironmentScene)) {
					window.PopScene(s);
				}
				
				window.ClearRenderQueues(-1); // just in case
				window.AddRenderQueue(-1, scene.Render);
				return;
			}
			
			window.AddRenderQueue(0, scene.Render);
		}

		public static void PopScene(this Window window, Scene scene) {
			if(!_windowScenes.ContainsKey(window)) return;
			if(!_windowScenes[window].Contains(scene)) return;
			
			_windowScenes[window].Remove(scene);
			
			var priority = scene is EnvironmentScene ? -1 : 0;
			window.RemoveRenderQueue(priority, scene.Render);
			
			scene.Deinitialize(window);
		}

		public static void PopAllScenes(this Window window) {
			foreach(var scene in new List<Scene>(window.GetScenes())) {
				window.PopScene(scene);
			}
		}

		public static IReadOnlyList<Scene> GetScenes(this Window window) {
			if(!_windowScenes.TryGetValue(window, out var scenes)) {
				return new List<Scene>();
			}

			return scenes;
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
