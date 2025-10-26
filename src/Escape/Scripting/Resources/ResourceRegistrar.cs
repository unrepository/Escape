using System.Runtime.CompilerServices;
using Escape.Resources;

namespace Escape.Scripting.Resources {
	
	public static class ResourceRegistrar {

		[ModuleInitializer]
		public static void Register() {
			ResourceRegistry.RegisterFormat<JSScript, JSScriptResource, JSScriptResource.Import>();
		}
	}
}
