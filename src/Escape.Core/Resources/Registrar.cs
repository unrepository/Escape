using System.Runtime.CompilerServices;
using Escape.Core.Input;
using Escape.Core.Scripting;
using Escape.Resources;

namespace Escape.Core.Resources {
	
	public static class Registrar {

		[ModuleInitializer]
		public static void Register() {
			ResourceRegistry.RegisterFormat<IScript, ScriptResource, ScriptResource.Import>();
			ResourceRegistry.RegisterFormat<Scene, SceneResource, SceneResource.Import>();
			ResourceRegistry.RegisterFormat<Dictionary<InputCombo[], InputAction>, ActionMapResource, ActionMapResource.Import>();
		}
	}
}
