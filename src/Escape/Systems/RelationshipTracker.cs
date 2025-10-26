using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Escape.Components;

namespace Escape.Systems {
	
	public class RelationshipTracker {

		public World World { get; }
		public bool Active { get; internal set; } = true;

		private readonly Dictionary<Entity, Child> _childEntities = [];
		
		public RelationshipTracker(World world) {
			World = world;

		#region Parent-Child
			world.SubscribeComponentAdded((in Entity e, ref Child c) => {
				if(!Active) return;
				
				ref var p = ref c.Parent.AddOrGet(new Parent([]));
				p._Children.Add(e);

				_childEntities[e] = c;
			});
			
			world.SubscribeComponentSet((in Entity e, ref Child c) => {
				if(!Active) return;

				ref var p = ref c.Parent.AddOrGet(new Parent([]));
				
				// remove from old parent
				if(_childEntities.TryGetValue(e, out var prevChild)) {
					prevChild.Parent.Get<Parent>()._Children.Remove(e);
				}
				
				p._Children.Add(e);
				_childEntities[e] = c;
			});
			
			world.SubscribeComponentRemoved((in Entity e, ref Child c) => {
				if(!Active) return;

				c.Parent.Get<Parent>()._Children.Remove(e);
				_childEntities.Remove(e);
			});
			
			// if entity has no parent initially, put it at the root
			world.SubscribeEntityCreated((in Entity e) => {
				if(!Active) return;

				if(!e.Has<Child>()) {
					e.MakeChildOf(world.GetRootEntity());
				}
			});
			
			// destroy all children once parent is destroyed
			world.SubscribeEntityDestroyed((in Entity e) => {
				if(!Active) return;

				foreach(var child in new List<Entity>(e.GetChildren())) {
					world.Destroy(child);
				}
				
				//world.Destroy(e);
				
				/*void Process(Entity e) {
					var children = e.GetChildren();
				
					foreach(var child in new List<Entity>(children)) {
						Process(child);
					}
					
					world.Destroy(e);
				}
				
				Process(e);*/
			});
		#endregion
		}
	}
}
