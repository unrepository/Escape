using System.Reflection;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Scripting.Resources {
	
	public class ScriptResource : Resource<IScript, ScriptResource.Import> {

		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".js", ".cs" ];
		
		public ScriptResource() { }
		public ScriptResource(IPlatform platform, string? filePath, JavaScriptScript value, Import? settings = null) : base(platform, filePath, value, settings) { }

		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings, bool reloading = false) {
			base.Load(platform, filePath, stream, resourceAssembly, settings, reloading);

			using var reader = new StreamReader(stream);
			
			switch(Path.GetExtension(filePath)) {
				case ".js":
					Value = new JavaScriptScript(filePath, reader.ReadToEnd());
					break;
				case ".cs":
					if(reloading) {
						var prevWorld = Value.World;
						var prevOwner = Value.Owner;
						
						Value = new CSharpScript(resourceAssembly, filePath, reader.ReadToEnd(), true);

						Value.World = prevWorld;
						Value.Owner = prevOwner;
					} else {
						Value = new CSharpScript(resourceAssembly, filePath, reader.ReadToEnd(), false);
					}
					break;
				default:
					throw new ArgumentException("Somehow, file doesn't have a valid extension", nameof(filePath));
			}
		}
		
		public override bool Save()
			=> base.Save();

		public override void SaveNew() {
			throw new NotImplementedException();
		}

		public override bool Reload()
			=> base.Reload();

		public override ScriptResource Duplicate() {
			using var stream = new FileStream(FilePath, FileMode.Open);

			var resource = new ScriptResource();
			resource.Load(Platform, FilePath, stream, ResourceAssembly, Settings);

			Duplicates.Add(resource);
			return resource;
		}

		public override void Dispose(bool reloading) {
			Value.Dispose();
			base.Dispose(reloading);
		}

		public class Import : ImportMetadata {

			public override string FormatId => "script";
		}
	}
}
