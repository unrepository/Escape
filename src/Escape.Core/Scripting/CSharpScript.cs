using System.Diagnostics;
using System.Reflection;
using Arch.Core;
using Escape.Renderer;
using Escape.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NLog;

namespace Escape.Core.Scripting {
	
	public class CSharpScript : IScript {

		public string Name { get; }
		public string Source { get; }
		//public IScript.Language Type => IScript.Language.CSharp;
		public bool IsInternal { get; } = false;
		
		public Type Type { get; }
		public object? Instance { get; private set; }

		public World World { get; set; }
		public Entity Owner { get; set; }

		protected Logger Logger { get; set; }

		private static Dictionary<Assembly, Assembly> _scriptsAssemblies = [];
		private static Dictionary<Assembly, List<string>> _loadedScripts = [];

		public CSharpScript() : this(null, "", "##INTERNAL") {
			Name = GetType().Name;
			IsInternal = true;

			Logger = LogManager.GetCurrentClassLogger();

			Type = GetType();
			Instance = this;
		}
		
		public CSharpScript(Assembly? scriptAssembly, string name, string source, bool reloading = false) {
			Name = name;
			Source = source;

			if(scriptAssembly is null || string.IsNullOrWhiteSpace(name)) return;

			Logger = LogManager.GetLogger(name.Replace(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "", ""));
			
			if(
				!_scriptsAssemblies.TryGetValue(scriptAssembly, out var scriptsAssembly)
				|| !_loadedScripts.TryGetValue(scriptAssembly, out var loadedScripts)
				|| !loadedScripts.Contains(name)
			) {
				_loadedScripts[scriptAssembly] = [];
				RebuildScripts(scriptAssembly);
			}

			scriptsAssembly = _scriptsAssemblies[scriptAssembly];

			foreach(var type in scriptsAssembly.GetExportedTypes()) {
				var scriptAttribute = type.GetCustomAttribute<CSharpScriptAttribute>();
				if(scriptAttribute is null) continue;
				
				var fullScriptPath = Path.Combine(ResourceManager.GetBaseDirectory(scriptAssembly), scriptAttribute.ScriptPath);
				
				if(fullScriptPath == name) {
					Type = type;
					break;
				}
			}

			if(Type is not null) {
				Logger.Warn(
					"Script {ScriptName} is not a CSharpScript; it will not be able to be called!",
					name
				);

				Instance = new CSharpScript();
			}
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
		public virtual void OnRender(RenderQueue queue, TimeSpan delta) { }

		public void Construct(Type[] types, object?[] arguments) {
			if(Instance is not null) throw new InvalidOperationException($"Script {Name} has already been constructed");
			
			var ctor = Type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, types);
			if(ctor is null) throw new InvalidDataException($"Script {Name} has no valid constructor for types [{string.Join(", ", types)}]");
					
			Instance = ctor.Invoke(arguments);

			var loggerProperty = Type.GetProperty("Logger", BindingFlags.Instance | BindingFlags.NonPublic);
			loggerProperty?.SetValue(Instance, Logger);
		}
		
		public object? Call(IScript.FunctionCall call, object?[] arguments) {
			var script = IsInternal ? this : (CSharpScript) Instance!;
			
			switch(call) {
				case IScript.FunctionCall.OnInitialize:
					script.OnInitialize((World) arguments[0], (Entity) arguments[1]);
					if(!IsInternal) OnInitialize((World) arguments[0], (Entity) arguments[1]);
					return null;
				case IScript.FunctionCall.OnDeinitialize:
					script.OnDeinitialize((World) arguments[0], (Entity) arguments[1]);
					if(!IsInternal) OnDeinitialize((World) arguments[0], (Entity) arguments[1]);
					return null;
				case IScript.FunctionCall.OnUpdate:
					script.OnUpdate((TimeSpan) arguments[0]);
					return null;
				case IScript.FunctionCall.OnRender:
					script.OnRender((RenderQueue) arguments[0], (TimeSpan) arguments[1]);
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
		
		public static void RebuildScripts(Assembly scriptAssembly) {
			var logger = LogManager.GetCurrentClassLogger();
			
			logger.Info("Rebuilding all external C# scripts...");
			var sw = Stopwatch.StartNew();
			
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
					logger.Trace("Could not add reference to {Assembly}: {Exception}", name, e.Message);
				}
			}

			// this assembly
			TryAddReference(Assembly.GetExecutingAssembly().GetName().Name);
			
			// + optional extensions
			TryAddReference("Escape.Extensions.Assimp");
			TryAddReference("Escape.Extensions.CSharp");
			TryAddReference("Escape.Extensions.Debugging");
			TryAddReference("Escape.Extensions.UI");
			TryAddReference("Escape.Extensions.Primitives");
			
			// + the project assembly
			TryAddReference(ESCAPE.ProjectAssembly.GetName().Name);
			
			// + the script assembly
			TryAddReference(scriptAssembly.GetName().Name);
			
			var compilation = CSharpCompilation
				.Create(scriptAssembly.GetName().Name)
				.WithOptions(
					new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
						.WithUsings("System", "Escape.Core", "Escape.Core.Components", "Escape.Core.Scripting", "Escape.Resources", "Arch.Core")
				)
				.AddReferences(references.Values.ToArray());

			foreach(
				var file in Directory.EnumerateFiles(
					ResourceManager.GetBaseDirectory(scriptAssembly), 
					"*.cs",
					SearchOption.AllDirectories
				)
			) {
				var preprocessorSymbols = new string[] {
					"SCRIPT"
				#if DEBUG
					, "DEBUG"
				#endif
				};
				
				var syntaxTree = CSharpSyntaxTree.ParseText(
					File.ReadAllText(file),
					CSharpParseOptions
						.Default
						.WithPreprocessorSymbols(preprocessorSymbols)
						.WithLanguageVersion(LanguageVersion.Preview),
					file
				);

				compilation = compilation.AddSyntaxTrees(syntaxTree);
				_loadedScripts[scriptAssembly].Add(file);
			}
			
			using var output = new MemoryStream();
			var result = compilation.Emit(output);

			if(!result.Success) {
				throw new Exception($"Script compilation failed: {string.Join("\n", result.Diagnostics)}");
			}

			output.Seek(0, SeekOrigin.Begin);

			var assembly = Assembly.Load(output.ToArray());
			_scriptsAssemblies[scriptAssembly] = assembly;
			
			sw.Stop();
			logger.Info("...Finished! in {Time}", sw.Elapsed);
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
