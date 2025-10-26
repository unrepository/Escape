using Arch.Core;
using Escape.Scripting.Components;

namespace Escape.Scripting.Systems {
	
	public class ScriptTracker {
		
		public World World { get; }
		public bool Active { get; internal set; } = true;

		public ScriptTracker(World world) {
			world.SubscribeComponentAdded((in Entity e, ref Scripted s) => {
				if(!Active) return;
				s.Script.Call(IScript.FunctionCall.OnInitialize, [ e ]);
			});
			
			world.SubscribeComponentRemoved((in Entity e, ref Scripted s) => {
				if(!Active) return;
				s.Script.Call(IScript.FunctionCall.OnDeinitialize, [ e ]);
			});
		}
	}
}
