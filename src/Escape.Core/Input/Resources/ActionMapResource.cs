using System.Reflection;
using System.Text.Json;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Core.Input.Resources {
	
	public class ActionMapResource : Resource<Dictionary<InputCombo[], InputAction>, ActionMapResource.Import> {

		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".actionmap" ];

		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings, bool reloading = false) {
			base.Load(platform, filePath, stream, resourceAssembly, settings, reloading);

			var dict = JsonSerializer.Deserialize<Dictionary<string, InputCombo[]>>(stream, ImportMetadata.DefaultSerializerOptions);
			if(dict is null) throw new InvalidDataException("Failed to deserialize input map");

			Value = [];

			foreach(var (actionName, combos) in dict) {
				var action = new InputAction(actionName);
				Value[combos] = action;
			}
		}

		public override bool Save() {
			if(!base.Save()) return false;

			var dict = new Dictionary<string, InputCombo[]>();

			foreach(var (combos, action) in Value) {
				dict[action.Name] = combos;
			}
			
			var json = JsonSerializer.Serialize(dict, ImportMetadata.DefaultSerializerOptions);
			File.WriteAllText(FilePath, json);

			return true;
		}

		public override void SaveNew() {
			throw new NotImplementedException();
		}
		
		public override ActionMapResource Duplicate() {
			using var stream = new FileStream(FilePath, FileMode.Open);

			var resource = new ActionMapResource();
			resource.Load(Platform, FilePath, stream, ResourceAssembly, Settings);

			Duplicates.Add(resource);
			return resource;
		}

		public class Import : ImportMetadata {

			public override string FormatId => "action_map";
		}
	}
}
