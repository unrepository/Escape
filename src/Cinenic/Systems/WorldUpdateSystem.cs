using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Cinenic.Components;

namespace Cinenic.Systems {
	
	public partial class WorldUpdateSystem : BaseSystem<World, TimeSpan> {

		public WorldUpdateSystem(World world) : base(world) { }
		
		[Query]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Camera3D_Update(ref Camera3D c3d) {
			if(!c3d.Enabled) return;
			c3d.Camera.Update();
		}

		[Query]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Transform3D_Camera3D_Synchronize(ref Camera3D c3d, ref Transform3D t3d) {
			c3d.Camera.Position = t3d.GlobalPosition;
			c3d.Camera.Target = t3d.GlobalPosition + Vector3.Transform(Vector3.UnitZ, t3d.GlobalRotation);
		}
	}
}
