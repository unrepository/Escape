using Cinenic.Renderer;

namespace Cinenic {
	
	public static class UpdateManager {

		public static Dictionary<string, IUpdateable> Updateables { get; } = [];
		public static Dictionary<string, bool> UpdateableStates { get; } = [];

		public static void Add(IUpdateable updateable, bool disabled = false) {
			Updateables[updateable.Id] = updateable;
			UpdateableStates[updateable.Id] = !disabled;
		}
		
		public static void Add(params IUpdateable[] updateables) {
			foreach(var updateable in updateables) {
				Updateables[updateable.Id] = updateable;
				UpdateableStates[updateable.Id] = true;
			}
		}

		public static IUpdateable? Get(string id) {
			if(Updateables.TryGetValue(id, out var updateable)) {
				return updateable;
			}

			return null;
		}
		
		public static bool IsEnabled(IUpdateable updateable) => UpdateableStates[updateable.Id];
		public static void SetEnabled(IUpdateable updateable, bool enabled) => UpdateableStates[updateable.Id] = enabled;
		public static void ToggleEnabled(IUpdateable updateable) => UpdateableStates[updateable.Id] = !UpdateableStates[updateable.Id];

		public static void Update(TimeSpan delta) {
			foreach(var updateable in Updateables.Values) {
				if(!IsEnabled(updateable)) continue;
				Update(updateable, delta);
			}
		}
		
		public static void Update(IUpdateable updateable, TimeSpan delta) {
			if(!IsEnabled(updateable)) return;
			updateable.Update(delta);
		}
	}
}
