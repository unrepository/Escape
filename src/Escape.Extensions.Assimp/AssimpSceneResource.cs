using System.Reflection;
using Escape.Extensions.Scene;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Extensions.Assimp {
	
	public class AssimpSceneResource : Resource<AssimpSceneResource.Import> {

		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [
			".glb", ".gltf",
			".3ds", ".ase",
			".3mf",
			".fbx",
			".mdl", ".md2", ".md3",
			".ply",
			".obj",
			".ter",
			".iqm",
			".smd", ".vta"
		];
		
		public AssimpScene? Scene { get; private set; }

		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings) {
			base.Load(platform, filePath, stream, resourceAssembly, settings);

			Scene = AssimpSceneLoader.Load(platform, filePath);
		}

		public override void Dispose(bool reloading) {
			Scene?.Dispose();
			base.Dispose(reloading);
		}
		
		public static implicit operator AssimpScene(AssimpSceneResource resource) => resource.Scene;

		public class Import : ImportMetadata {

			public override string FormatId => "assimp_scene";
		}
	}
}
