using System.Runtime.CompilerServices;
using Escape.Resources;

namespace Escape.Scripting.Resources {
	
	public static class Registrar {

		[ModuleInitializer]
		public static void Register() {
			ResourceRegistry.RegisterFormat<IScript, ScriptResource, ScriptResource.Import>();
		}
	}
}
