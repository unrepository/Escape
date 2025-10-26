using Escape.Renderer;

namespace Escape {
	
	public static class SceneEngine {

		private static readonly Dictionary<RenderQueue, Scene> _scenes = [];

		public static void SetScene(RenderQueue renderQueue, Scene? scene) {
			if(_scenes.TryGetValue(renderQueue, out var openScene)) {
				openScene.Close();
			}

			if(scene is null) {
				_scenes.Remove(renderQueue);
				return;
			}

			_scenes[renderQueue] = scene;
			scene.Open();
		}

		public static Scene? GetScene(RenderQueue renderQueue) {
			if(_scenes.TryGetValue(renderQueue, out var scene)) {
				return scene;
			}

			return null;
		}
	}
}
