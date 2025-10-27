using Arch.Core;
using Escape.Core.Scripting.Components;
using Escape.Core.Systems;

namespace Escape.Core.Scripting.Systems {
	
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
