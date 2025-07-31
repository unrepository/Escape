using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Visio.Renderer;
using Visio.Components;

namespace Visio.Systems {
	
	public partial class RenderUpdateSystem : BaseSystem<World, TimeSpan> {

		private ObjectRenderer _objectRenderer;

		public RenderUpdateSystem(World world, ObjectRenderer objectRenderer) : base(world) {
			_objectRenderer = objectRenderer;
		}

		[Query]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Transform3D_Update(ref RenderableObject obj, ref Transform3D t3d) {
			_objectRenderer.SetMatrix(obj, t3d.GlobalMatrix);
		}
	}
}
