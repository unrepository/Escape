using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Escape.Core.Components;
using Escape.Core.Scripting;
using Escape.Core.Scripting.Components;
using Escape.Renderer;

namespace Escape.Core.Systems {
	
	public partial class RenderUpdateSystem : BaseSystem<World, TimeSpan> {

		private ObjectRenderer _objectRenderer;

		public RenderUpdateSystem(World world, ObjectRenderer objectRenderer) : base(world) {
			_objectRenderer = objectRenderer;
		}

		[Query]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Transform3D_Update(in Entity e, ref Renderable obj, ref Transform3D t3d) {
			if(e.IsDisabled()) return;
			_objectRenderer.SetMatrix(obj, t3d.GlobalMatrix);
		}
		
		/*[Query]
		public void Scripted_Render(in Entity e, ref Scripted scripted) {
			if(e.IsDisabled() || !e.IsVisible()) return;
			scripted.Script.Call(IScript.FunctionCall.OnRender, [ ESCAPE.RenderDelta, _objectRenderer ]);
		}*/
	}
}
