using Cinenic.Renderer;
using Cinenic.World;

namespace Cinenic {
	
	public static class UpdateManager {

		public static Dictionary<string, IUpdater> Updateables { get; } = [];
		public static Dictionary<string, bool> UpdateableStates { get; } = [];

		public static void Add(IUpdater updateable, bool disabled = false) {
			Updateables[updateable.Id] = updateable;
			UpdateableStates[updateable.Id] = !disabled;
		}
		
		public static void Add(params IUpdater[] updateables) {
			foreach(var updateable in updateables) {
				Updateables[updateable.Id] = updateable;
				UpdateableStates[updateable.Id] = true;
			}
		}

		public static IUpdater? Get(string id) {
			if(Updateables.TryGetValue(id, out var updateable)) {
				return updateable;
			}

			return null;
		}
		
		public static bool IsEnabled(IUpdater updateable) => UpdateableStates[updateable.Id];
		public static void SetEnabled(IUpdater updateable, bool enabled) => UpdateableStates[updateable.Id] = enabled;
		public static void ToggleEnabled(IUpdater updateable) => UpdateableStates[updateable.Id] = !UpdateableStates[updateable.Id];

		public static void Update(TimeSpan delta) {
			foreach(var updateable in Updateables.Values) {
				if(!IsEnabled(updateable)) continue;
				Update(updateable, delta);
			}
		}
		
		public static void Update(IUpdater updateable, TimeSpan delta) {
			if(!IsEnabled(updateable)) return;
			updateable.Update(delta);
		}
	}
}
