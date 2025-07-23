using System.Runtime.CompilerServices;
using Cinenic.Resources;

namespace Cinenic.Extensions.Assimp {
	
	public static class ResourceRegister {

		[ModuleInitializer]
		public static void Initialize() {
			ResourceRegistry.RegisterFormat<AssimpSceneResource, AssimpSceneResource.Import>();
		}
	}
}
