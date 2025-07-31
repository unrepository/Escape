using System.Runtime.CompilerServices;
using Visio.Resources;

namespace Visio.Extensions.Assimp {
	
	public static class ResourceRegister {

		[ModuleInitializer]
		public static void Initialize() {
			ResourceRegistry.RegisterFormat<AssimpSceneResource, AssimpSceneResource.Import>();
		}
	}
}
