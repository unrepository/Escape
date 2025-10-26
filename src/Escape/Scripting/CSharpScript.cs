using System.Diagnostics;
using System.Reflection;
using Arch.Core;
using Escape.Renderer;
using Microsoft.CodeAnalysis.Scripting;

namespace Escape.Scripting {
	
	public class CSharpScript : IScript {

		public string Name { get; }
		public string Source { get; }
		public IScript.Language Type => IScript.Language.CSharp;
		public bool IsInternal { get; } = false;
		
		public Script? Script { get; private set; }
		public object? Instance { get; private set; }

		protected Entity Owner { get; private set; }

		private static readonly ScriptOptions DefaultEnvironment;

		static CSharpScript() {
			var assemblies = new List<Assembly>();

			void TryLoadAssembly(string name) {
				try {
					assemblies.Add(Assembly.Load(name));
				} catch(Exception) { }
			}
			
			TryLoadAssembly("System");
			TryLoadAssembly("Escape");
			TryLoadAssembly("Escape.Renderer");
			TryLoadAssembly("Escape.Resources");
			TryLoadAssembly("Escape.UnitTypes");
			TryLoadAssembly("Escape.Primitives");
			TryLoadAssembly("Escape.Extensions.Scene");
			TryLoadAssembly("Escape.Extensions.Debugging");
			TryLoadAssembly("Escape.Extensions.ImGui");
			TryLoadAssembly("Escape.Extensions.Assimp");
			TryLoadAssembly("Arch.Core");
			TryLoadAssembly("Arch.Core.Extensions");
			
			DefaultEnvironment =
				ScriptOptions
					.Default
					.AddImports("System", "Escape", "Escape.Scripting", "Escape.Components", "Escape.UnitTypes", "Escape.Resources", "Arch.Core")
					.AddReferences(assemblies.ToArray());
		}

		public CSharpScript() {
			Name = GetType().Name;
			Source = "##INTERNAL";
			IsInternal = true;
		}
		
		public CSharpScript(string name, string source) {
			Name = name;
			Source = source;

			Instance = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync(
				source,
				DefaultEnvironment
			).GetAwaiter().GetResult();
		}

		public virtual void OnInitialize(Entity e) {
			Owner = e;
		}

		public virtual void OnDeinitialize(Entity e) {
			Owner = default;
		}
		
		public virtual void OnUpdate(TimeSpan delta) { }
		public virtual void OnRender(TimeSpan delta, ObjectRenderer objectRenderer) { }

		public void Compile() {
			Script = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.Create(Source, DefaultEnvironment);
			Script.Compile();
		}
		
		public object? Call(IScript.FunctionCall call, object?[] arguments) {
			var script = IsInternal ? this : (CSharpScript) Instance!;
			
			switch(call) {
				case IScript.FunctionCall.OnInitialize:
					script.OnInitialize((Entity) arguments[0]);
					return null;
				case IScript.FunctionCall.OnDeinitialize:
					script.OnDeinitialize((Entity) arguments[0]);
					return null;
				case IScript.FunctionCall.OnUpdate:
					script.OnUpdate((TimeSpan) arguments[0]);
					return null;
				case IScript.FunctionCall.OnRender:
					script.OnRender((TimeSpan) arguments[0], (ObjectRenderer) arguments[1]);
					return null;
				default:
					throw new NotImplementedException();
			}
		}

		public object? Call(string function, object?[] arguments)
			=> throw new NotImplementedException();

		public void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
