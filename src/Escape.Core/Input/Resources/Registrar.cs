using System.Runtime.CompilerServices;
using Escape.Resources;

namespace Escape.Core.Input.Resources {
	
	public static class Registrar {

		[ModuleInitializer]
		public static void Register() {
			ResourceRegistry.RegisterFormat<Dictionary<InputCombo[], InputAction>, ActionMapResource, ActionMapResource.Import>();
		}
	}
}
