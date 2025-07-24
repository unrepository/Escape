using System.Diagnostics;
using Arch.Core;
using Arch.Core.Extensions;

namespace Cinenic.Components {

	/// <summary>
	/// Indicates that this entity is a parent of another entity
	/// </summary>
	[Component]
	public struct Parent {

		public IReadOnlyList<Entity> Children => _Children;
		internal List<Entity> _Children { get; set; }

		public Parent() {
			throw new NotSupportedException("The Parent component is not meant to be created manually!");
		}
		
		internal Parent(List<Entity> children) {
			_Children = children;
		}
	}

	/// <summary>
	/// Indicates that this entity is a child of another entity
	/// </summary>
	[Component]
	public record struct Child(Entity Parent);

	public static class Relationship {

		/// <summary>
		/// Makes an entity a child to another entity or removes it as a child from any parent
		/// </summary>
		/// <param name="entity">The entity that should become or stop being a child</param>
		/// <param name="parent">The parent entity for the child to belong to. If null, it will make the entity independent.</param>
		public static void MakeChildOf(this Entity entity, Entity? parent) {
			if(!entity.Has<Child>()) {
				if(parent is null) return;
				entity.Add(new Child(parent.Value));
				return;
			}
			
			if(parent is null) {
				entity.Remove<Child>();
				return;
			}
			
			entity.Set(new Child(parent.Value));
		}
		
		public static void MakeParentOf(this Entity entity, params Entity[] children) {
			Debug.Assert(children.Length > 0);
			entity.MakeParentOf((IEnumerable<Entity>) children);
		}

		public static void MakeParentOf(this Entity entity, IEnumerable<Entity>? children) {
			if(children is null) {
				ref var p = ref entity.AddOrGet(new Parent([]));

				foreach(var child in p.Children) {
					child.Remove<Child>();
				}

				return;
			}

			foreach(var child in children) {
				child.MakeChildOf(entity);
			}
		}

		public static IReadOnlyList<Entity> GetChildren(this Entity entity) {
			if(!entity.Has<Parent>()) return [];
			return entity.Get<Parent>().Children;
		}

		public static Entity GetChild(this Entity entity, int id = -1, int index = -1) {
			if((id < 0 && index < 0) | (id > 0 && index > 0)) throw new ArgumentException("Only one of id or index must be specified");

			var children = entity.GetChildren();

			if(index > 0) {
				return children[index];
			}

			return children.Single(e => e.Id == id);
		}

		public static Entity? GetParent(this Entity entity) {
			if(!entity.Has<Child>()) return null;
			return entity.Get<Child>().Parent;
		}

		public static bool HasChildren(this Entity entity) => entity.Has<Parent>() && entity.Get<Parent>().Children.Count > 0;
		public static bool HasParent(this Entity entity) => entity.Has<Child>();
	}
}
