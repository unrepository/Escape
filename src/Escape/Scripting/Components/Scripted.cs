using Escape.Components;
using Escape.Resources;
using Escape.Scripting.Resources;

namespace Escape.Scripting.Components {

	[Component]
	public struct Scripted {

		public Ref<ScriptResource>? ResourceScript { get; }
		public IScript? InternalScript { get; }

		public IScript Script => ResourceScript?.Get().Value ?? InternalScript!;

		public Scripted(Ref<ScriptResource> script) {
			ResourceScript = script;
		}

		public Scripted(IScript script) {
			InternalScript = script;
		}
	}
}
