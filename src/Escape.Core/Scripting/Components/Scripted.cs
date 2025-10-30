using Escape.Core.Components;
using Escape.Core.Resources;
using Escape.Resources;

namespace Escape.Core.Scripting.Components {

	[Component]
	public struct Scripted {

		public Ref<ScriptResource>? ResourceScript { get; private set; }
		public IScript? InternalScript { get; }
		
		public Type[] ConstructorTypes { get; set; }
		public object?[] ConstructorArguments { get; set; }

		public IScript Script => ResourceScript?.Get().Value ?? InternalScript!;

		public Scripted(Ref<ScriptResource> script, params object?[] arguments) 
			: this(script, arguments.OfType<object>().Select(argument => argument.GetType()).ToArray(), arguments) { }
		
		public Scripted(Ref<ScriptResource> script, Type[] types, params object?[] arguments) {
			ResourceScript = script;
			ConstructorTypes = types;
			ConstructorArguments = arguments;

			var t = this;
			
			try {
				Script.Construct(ConstructorTypes, ConstructorArguments);
			} catch(InvalidOperationException) {
				ResourceScript = new(ResourceScript.Get().Duplicate());
				Script.Construct(ConstructorTypes, ConstructorArguments);
			}
			
			ResourceScript.Get().Reloaded += res => {
				t.ResourceScript = new Ref<ScriptResource>((ScriptResource) res);
				
				res.Value.Construct(t.ConstructorTypes, t.ConstructorArguments);
				res.Value.Call(IScript.FunctionCall.OnInitialize, [ res.Value.World, res.Value.Owner ]);
			};
		}

		public Scripted(IScript script) {
			InternalScript = script;
		}
	}
}
