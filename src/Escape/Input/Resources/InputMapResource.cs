using System.Reflection;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Input.Resources {
	
	public class InputMapResource : Resource<InputMap, InputMapResource.Import> {

		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".inputmap" ];

		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings, bool reloading = false) {
			base.Load(platform, filePath, stream, resourceAssembly, settings, reloading);
		}

		public override void SaveNew() {
			throw new NotImplementedException();
		}
		
		public override InputMapResource Duplicate() {
			using var stream = new FileStream(FilePath, FileMode.Open);

			var resource = new InputMapResource();
			resource.Load(Platform, FilePath, stream, ResourceAssembly, Settings);

			Duplicates.Add(resource);
			return resource;
		}

		public class Import : ImportMetadata {

			public override string FormatId => "input_map";
		}
	}
}
