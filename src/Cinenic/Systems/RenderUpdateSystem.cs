using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Cinenic.Components;
using Cinenic.Renderer;

namespace Cinenic.Systems {
	
	public partial class RenderUpdateSystem : BaseSystem<World, TimeSpan> {

		private ObjectRenderer _objectRenderer;

		public RenderUpdateSystem(World world, ObjectRenderer objectRenderer) : base(world) {
			_objectRenderer = objectRenderer;
		}
		
		// NOTE: this can be in the update thread, but camera movement can be choppy if it's out of sync with the framerate
		[Query]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Camera3D_Update(ref Camera3D c3d) {
			if(!c3d.Enabled) return;
			c3d.Camera.Update();
		}

		// NOTE: this can be in the update thread, but camera movement can be choppy if it's out of sync with the framerate
		[Query]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Transform3D_Camera3D_Synchronize(ref Camera3D c3d, ref Transform3D t3d) {
			c3d.Camera.Position = t3d.Position;
			c3d.Camera.Target = t3d.Position + Vector3.Transform(Vector3.UnitZ, t3d.Rotation);
		}

		[Query]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Transform3D_Update(ref RenderableObject obj, ref Transform3D t3d) {
			var matrix =
				Matrix4x4.CreateScale(t3d.Scale)
				* Matrix4x4.CreateFromQuaternion(t3d.Rotation)
				* Matrix4x4.CreateTranslation(t3d.Position);

			_objectRenderer.SetMatrix(obj, matrix);
		}
	}
}
