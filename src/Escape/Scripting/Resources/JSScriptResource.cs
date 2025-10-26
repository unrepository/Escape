using System.Reflection;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Scripting.Resources {
	
	public class JSScriptResource : Resource<JSScript, JSScriptResource.Import> {

		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".js" ];
		
		public JSScriptResource() { }
		public JSScriptResource(IPlatform platform, string? filePath, JSScript value, Import? settings = null) : base(platform, filePath, value, settings) { }

		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings) {
			base.Load(platform, filePath, stream, resourceAssembly, settings);

			using(var reader = new StreamReader(stream)) {
				Value = new JSScript(filePath, reader.ReadToEnd());
			}
		}
		
		public override bool Save()
			=> base.Save();

		public override void SaveNew() {
			throw new NotImplementedException();
		}

		public override bool Reload()
			=> base.Reload();
		
		public override void Dispose(bool reloading) {
			Value.Dispose();
			base.Dispose(reloading);
		}

		public class Import : ImportMetadata {

			public override string FormatId => "jsscript";
		}
	}
}
