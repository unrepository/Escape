using Arch.Core;
using Escape.Renderer;

namespace Escape.Scripting {
	
	public class CSharpScript : IScript {

		public string Name { get; }
		public string Source { get; }
		public bool IsInternal { get; } = false;
		
		public IScript.Language Type => IScript.Language.CSharp;

		protected Entity Owner { get; private set; }

		public CSharpScript() {
			Name = GetType().Name;
			Source = "##INTERNAL";
			IsInternal = true;
		}
		
		public CSharpScript(string name, string source) {
			Name = name;
			Source = source;
			
			// TODO compile script
			throw new NotImplementedException();
		}

		public virtual void OnInitialize(Entity e) {
			Owner = e;
		}

		public virtual void OnDeinitialize(Entity e) {
			Owner = default;
		}
		
		public virtual void OnUpdate(TimeSpan delta) { }
		public virtual void OnRender(TimeSpan delta, ObjectRenderer objectRenderer) { }

		public object? Call(IScript.FunctionCall function, object?[] arguments) {
			if(IsInternal) {
				switch(function) {
					case IScript.FunctionCall.OnInitialize:
						OnInitialize((Entity) arguments[0]);
						return null;
					case IScript.FunctionCall.OnDeinitialize:
						OnDeinitialize((Entity) arguments[0]);
						return null;
					case IScript.FunctionCall.OnUpdate:
						OnUpdate((TimeSpan) arguments[0]);
						return null;
					case IScript.FunctionCall.OnRender:
						OnRender((TimeSpan) arguments[0], (ObjectRenderer) arguments[1]);
						return null;
					default:
						throw new NotImplementedException();
				}
			}

			throw new NotImplementedException();
		}

		public object? Call(string function, object?[] arguments)
			=> throw new NotImplementedException();

		public void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
