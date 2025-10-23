using System.Runtime.CompilerServices;
using Escape.Resources;

namespace Escape.Extensions.Assimp {
	
	public static class ResourceRegister {

		[ModuleInitializer]
		public static void Initialize() {
			ResourceRegistry.RegisterFormat<AssimpSceneResource, AssimpSceneResource.Import>();
		}
	}
}
