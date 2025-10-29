using Arch.Core;
using Arch.Core.Extensions;
using Escape.Core.Components;

namespace Escape.Core.Systems {
	
	public class StateTracker : TrackerSystem {

		public StateTracker(World world, bool active = true) : base(world, active) {
			world.SubscribeEntityCreated((in Entity e) => {
				if(!e.Has<State>()) e.Add(new State());
			});
			
			world.SubscribeComponentAdded((in Entity e, ref State state) => {
				state.Owner = e;
			});
			
			world.SubscribeComponentSet((in Entity e, ref State state) => {
				state.Owner = e;
			});
			
			world.SubscribeComponentRemoved((in Entity e, ref State state) => {
				e.Add(new State());
			});
		}
	}
}
