using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Cinenic.Components;

namespace Cinenic.Systems {
	
	public class HierarchyUpdateSystem : BaseSystem<World, TimeSpan> {

		public Entity Root { get; }
		public bool DebugPrintHierarchy { get; set; } = false;

		public HierarchyUpdateSystem(World world) : base(world) {
			Root = world.GetRootEntity();
		}

		public override void Update(in TimeSpan delta) {
			void ProcessEntity(Entity parent, Entity entity, bool parentDirty, int depth) {
			#if DEBUG
				if(DebugPrintHierarchy) {
					Console.Write(new string(' ', depth * 2));
					Console.Write($"- {entity.Id}");
				
					if (entity.Has<Transform3D>()) {
						var t3d = entity.Get<Transform3D>();
						
						Console.Write(' ');
						Console.Write(t3d.ToString());
					} else {
						Console.Write(" [No Transform3D]");
					}
				
					Console.WriteLine();
				}
			#endif
				
				bool isDirty = parentDirty;

				if (entity.Has<Transform3D>()) {
					ref var t3d = ref entity.Get<Transform3D>();

					// entity is dirty if it itself is dirty OR its parent was dirty
					isDirty |= t3d.IsDirty;

					if (isDirty) {
						t3d.LocalMatrix = Transform3D.CreateMatrix(t3d);

						if (parent.Has<Transform3D>()) {
							ref var pt3d = ref parent.Get<Transform3D>();
							t3d.GlobalMatrix = t3d.LocalMatrix * pt3d.GlobalMatrix;
						} else {
							t3d.GlobalMatrix = t3d.LocalMatrix;
						}

						t3d.IsDirty = false;
					}
				}

				foreach (var child in entity.GetChildren()) {
					ProcessEntity(entity, child, isDirty, depth + 1);
				}
			}

			ProcessEntity(default, Root, false, 0);
		}
	}
}
