using System.Reflection;
using Acornima.Ast;
using Arch.Core;
using Arch.Core.Extensions;
using Escape.Core.Components;
using Jint;
using Jint.Native.Object;
using Jint.Runtime;
using NLog;

namespace Escape.Core.Scripting {
	
	public class JavaScriptScript : IScript, IDisposable {
		
		public string Name { get; }
		public string Source { get; }

		//public IScript.Language Type => IScript.Language.JavaScript;
		
		public World World { get; set; }
		public Entity Owner { get; set; }

		public Prepared<Script>? Script { get; private set; }
		public ObjectInstance Module { get; private set; }
		
		private readonly Logger _scriptLogger;
		
		private Engine? _engine;
		private Thread? _engineThread;
		
		public JavaScriptScript(string name, string source) {
			throw new NotImplementedException();
			
			Name = name;
			Source = source;
			
		#if !DEBUG
			Script = Engine.PrepareScript(
				Source,
				Name,
				true,
				new ScriptPreparationOptions() {
					ParsingOptions = new ScriptParsingOptions() {
						CompileRegex = true,
					},
					FoldConstants = true
				}
			);
		#endif

			_scriptLogger = LogManager.GetLogger(name);
			
			_CreateEngine();
		}

		public void Construct(Type[] types, object?[] arguments) {
			throw new NotImplementedException();
		}

		public object? Call(IScript.FunctionCall call, object?[] arguments) {
			var function = call switch {
				IScript.FunctionCall.OnInitialize => "init",
				IScript.FunctionCall.OnDeinitialize => "deinit",
				IScript.FunctionCall.OnUpdate => "update",
				IScript.FunctionCall.OnRender => "render",
				_ => throw new NotImplementedException()
			};

			return Call(function, arguments);
		}

		public object? Call(string function, object?[] arguments) {
			_CreateEngine();
			
			try {
				return Module.Get(function);
				//return _engine!.Invoke(function, null, arguments);
			} catch(JavaScriptException e) {
				_scriptLogger.Error("Could not call function {Function}: {Exception}", function, e.ToString());
			}

			return null;
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			_engine?.Dispose();
		}

		private void _CreateEngine() {
			var assemblies = new List<Assembly>();

			void TryLoadAssembly(string name) {
				try {
					assemblies.Add(Assembly.Load(name));
				} catch(Exception) { }
			}
			
			TryLoadAssembly("System");
			TryLoadAssembly("Escape.Core");
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
			
			var cfg = (Options cfg) => {
				cfg.AllowClr(assemblies.ToArray());
			};
			
			if(_engine is null) {
				_engine = new Engine(cfg);
				_engineThread = Thread.CurrentThread;
			} else if(_engineThread != Thread.CurrentThread) {
				_engine.Dispose();
				
				_engine = new Engine(cfg);
				_engineThread = Thread.CurrentThread;
			}

		#region Default environment
			_engine.SetValue("print", (object o) => {
				Console.Write(o);
			});
			
			_engine.SetValue("println", (object o) => {
				Console.WriteLine(o);
			});
			
			_engine.SetValue("message", (object o) => {
				_scriptLogger.Info(o);
			});

			_engine.SetValue("getComponent", (Entity e, Type t) => {
				return e.Get(t);
			});
			
			_engine.Modules.Add("enginelib", builder =>
				builder
					.ExportType<Transform3D>()
			);
		#endregion
			
			// prepare script
			//if(Script is not null) _engine!.Execute(Script.Value);
			//else _engine!.Execute(Source, Name);
			
			_engine.Modules.Add("script", Source);
			Module = _engine.Modules.Import("script");
		}
	}
}
