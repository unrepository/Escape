using Arch.Core;
using Escape.Scripting.Components;
using Escape.Systems;

namespace Escape.Scripting.Systems {
	
	public class ScriptTracker : TrackerSystem {

		public ScriptTracker(World world) : base(world) {
			world.SubscribeComponentAdded((in Entity e, ref Scripted s) => {
				if(!Active) return;
				s.Script.Call(IScript.FunctionCall.OnInitialize, [ world, e ]);
			});
			
			world.SubscribeComponentRemoved((in Entity e, ref Scripted s) => {
				if(!Active) return;
				s.Script.Call(IScript.FunctionCall.OnDeinitialize, [ world, e ]);
			});
		}
	}
}
