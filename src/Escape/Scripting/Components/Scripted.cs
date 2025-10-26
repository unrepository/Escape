using Escape.Components;
using Escape.Resources;
using Escape.Scripting.Resources;

namespace Escape.Scripting.Components {

	[Component]
	public record struct Scripted(Ref<JSScriptResource> Script, string? EntryPoint);
}
