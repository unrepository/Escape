using System.Text.Json.Serialization;
using Arch.Core;
using Arch.Core.Extensions;

namespace Escape.Core.Components {

	[Component]
	public struct State {
		
		[JsonIgnore]
		public Entity Owner { get; internal set; }

		public string? Name {
			get;
			set {
				if(value == null) {
					field = value;
					return;
				}
				
				// check for any name conflicts
				var w = Owner.GetWorld();
				var query = w.Query(new QueryDescription().WithAny<State>());

				foreach(ref var chunk in query) {
					var states = chunk.GetSpan<State>();

					foreach(var entityIndex in chunk) {
						ref var state = ref states[entityIndex];

						if(state.Name == value) {
							throw new ArgumentException("An entity with the same name already exists");
						}
					}
				}

				field = value;
			}
		}

		public bool Visible { get; set; }
		public bool Disabled { get; set; }

		public State() {
			Name = null;
			Visible = true;
			Disabled = false;
		}

		public State(string? name = null, bool visible = true, bool disabled = false) {
			Name = name;
			Visible = visible;
			Disabled = disabled;
		}
	}

	public static class StateExtensions {

		public static string? GetName(this Entity e) => e.Get<State>().Name;

		public static Entity? GetEntityByName(this World w, string name) {
			var query = w.Query(new QueryDescription().WithAny<State>());

			Entity? result = null;
			
			foreach(ref var chunk in query) {
				var states = chunk.GetSpan<State>();

				foreach(var entityIndex in chunk) {
					ref var state = ref states[entityIndex];

					if(state.Name == name) {
						result = chunk.Entities[entityIndex];
						break;
					}
				}

				if(result is not null) break;
			}

			return result;
		}

		public static bool IsVisible(this Entity e) => e.Get<State>().Visible;
		public static void SetVisible(this Entity e, bool visible, bool children = true) {
			e.Get<State>().Visible = visible;
			if(!children) return;
			
			foreach(var child in e.GetChildren()) {
				child.SetVisible(visible, true);
			}
		}
		
		public static bool IsDisabled(this Entity e) => e.Get<State>().Disabled;
		public static void SetDisabled(this Entity e, bool disabled, bool children = true) {
			e.Get<State>().Disabled = disabled;
			if(!children) return;
			
			foreach(var child in e.GetChildren()) {
				child.SetDisabled(disabled, true);
			}
		}

		public static World GetWorld(this Entity e) {
			foreach(var world in World.Worlds) {
				if(world.Id == e.WorldId) {
					return world;
				}
			}

			throw new InvalidDataException("how the heck can an entity not belong to a world");
		}
	}
}
