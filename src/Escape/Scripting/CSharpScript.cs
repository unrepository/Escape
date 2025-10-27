using System.Diagnostics;
using System.Reflection;
using Arch.Core;
using Escape.Renderer;
using Escape.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NLog;

namespace Escape.Scripting {
	
	public class CSharpScript : IScript {

		public string Name { get; }
		public string Source { get; }
		//public IScript.Language Type => IScript.Language.CSharp;
		public bool IsInternal { get; } = false;
		
		public Type Type { get; private set; }
		public object? Instance { get; private set; }

		protected World World { get; private set; }
		protected Entity Owner { get; private set; }
		
		protected Logger Logger { get; }

		private static Assembly? _scriptsAssembly;
		private static List<string> _loadedScripts = [];

		public CSharpScript() : this(null, "", "##INTERNAL") {
			Name = GetType().Name;
			IsInternal = true;

			Logger = LogManager.GetCurrentClassLogger();

			Type = GetType();
			Instance = this;
		}
		
		public CSharpScript(Assembly? scriptAssembly, string name, string source) {
			Name = name;
			Source = source;
			Logger = LogManager.GetLogger(name);

			if(scriptAssembly is null || string.IsNullOrWhiteSpace(name)) return;

			if(_scriptsAssembly is null || !_loadedScripts.Contains(name)) {
				RebuildScripts();
			}

			foreach(var type in _scriptsAssembly!.GetExportedTypes()) {
				var scriptAttribute = type.GetCustomAttribute<CSharpScriptAttribute>();
				if(scriptAttribute is null) continue;
				
				var fullScriptPath = Path.Combine(ResourceManager.GetBaseDirectory(scriptAssembly), scriptAttribute.ScriptPath);
				
				if(fullScriptPath == name) {
					Type = type;
					Instance = type.GetConstructor([]).Invoke(null);
				}
			}
			
			Debug.Assert(Type is not null);
			Debug.Assert(Instance is not null);
		}

		public virtual void OnInitialize(World w, Entity e) {
			World = w;
			Owner = e;
		}

		public virtual void OnDeinitialize(World w, Entity e) {
			World = w;
			Owner = default;
		}
		
		public virtual void OnUpdate(TimeSpan delta) { }
		public virtual void OnRender(TimeSpan delta, ObjectRenderer objectRenderer) { }

		public void RebuildScripts() {
			var references = new Dictionary<string, MetadataReference>();

			void TryAddReference(string name) {
				if(references.ContainsKey(name)) return;
				
				try {
					var assembly = Assembly.Load(name);
					
					// the assembly itself
					references[name] = MetadataReference.CreateFromFile(assembly.Location);
					
					// + everything else it might reference (depend on)
					foreach(var dependency in assembly.GetReferencedAssemblies()) {
						TryAddReference(dependency.Name);
					}
				} catch(Exception e) {
					Logger.Trace("Could not add reference to {Assembly}: {Exception}", name, e.Message);
				}
			}

			// this assembly
			TryAddReference(Assembly.GetExecutingAssembly().GetName().Name);
			
			// + optional extensions
			TryAddReference("Escape.Extensions.Assimp");
			TryAddReference("Escape.Extensions.CSharp");
			TryAddReference("Escape.Extensions.Debugging");
			TryAddReference("Escape.Extensions.ImGui");
			TryAddReference("Escape.Primitives");
			
			// + the project assembly
			TryAddReference(ESCAPE.ProjectAssembly.GetName().Name);
			
			var compilation = CSharpCompilation
				.Create(ESCAPE.ProjectAssembly.GetName().Name)
				.WithOptions(
					new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
						.WithUsings("System", "Escape", "Escape.Components", "Escape.Scripting", "Escape.Resources", "Arch.Core")
				)
				.AddReferences(references.Values.ToArray());

			foreach(
				var file in Directory.EnumerateFiles(
					ResourceManager.GetBaseDirectory(ESCAPE.ProjectAssembly), 
					"*.cs",
					SearchOption.AllDirectories
				)
			) {
				var syntaxTree = CSharpSyntaxTree.ParseText(
					File.ReadAllText(file),
					CSharpParseOptions
						.Default
						.WithPreprocessorSymbols("SCRIPT")
						.WithLanguageVersion(LanguageVersion.Preview),
					Name
				);

				compilation = compilation.AddSyntaxTrees(syntaxTree);
				_loadedScripts.Add(file);
			}
			
			using var output = new MemoryStream();
			var result = compilation.Emit(output);

			if(!result.Success) {
				throw new Exception($"Script compilation failed: {string.Join("\n", result.Diagnostics)}");
			}

			output.Seek(0, SeekOrigin.Begin);

			var assembly = Assembly.Load(output.ToArray());
			_scriptsAssembly = assembly;
		}
		
		public object? Call(IScript.FunctionCall call, object?[] arguments) {
			var script = IsInternal ? this : (CSharpScript) Instance!;
			
			switch(call) {
				case IScript.FunctionCall.OnInitialize:
					script.OnInitialize((World) arguments[0], (Entity) arguments[1]);
					return null;
				case IScript.FunctionCall.OnDeinitialize:
					script.OnDeinitialize((World) arguments[0], (Entity) arguments[1]);
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

		public object? Call(string function, object?[] arguments) {
			var method = Type.GetMethod(function, BindingFlags.Public | BindingFlags.IgnoreCase);
			return method?.Invoke(Instance, arguments) ?? null;
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
		}
	}

    [AttributeUsage(AttributeTargets.Class)]
    public class CSharpScriptAttribute : Attribute {
		
		public string ScriptPath { get; }

		public CSharpScriptAttribute(string scriptPath) {
			ScriptPath = scriptPath;
		}
	}
}
