using System.Reflection;
using Escape.Renderer;
using Escape.Resources;

namespace Escape.Renderer.Resources {
	
	public class TextureResource : Resource<Texture, TextureResource.Import> {

		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".png", ".jpg", ".bmp", ".jpeg", ".tiff", ".tga", ".webp", ".pbm", ".qoi" ];

		public Texture? Texture { get; private set; }
		
		public TextureResource() { }
		public TextureResource(IPlatform platform, string? filePath, Texture value, Import? settings = null) : base(platform, filePath, value, settings) { }
		
		public override void SaveNew() {
			throw new NotImplementedException();
		}
		
		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings, bool reloading = false) {
			base.Load(platform, filePath, stream, resourceAssembly, settings, reloading);
			
			Texture = Texture.Create(
				platform,
				stream,
				Settings.Filter,
				Settings.WrapMode,
				Settings.Format
			);
		}
		
		public override TextureResource Duplicate() {
			using var stream = new FileStream(FilePath, FileMode.Open);

			var resource = new TextureResource();
			resource.Load(Platform, FilePath, stream, ResourceAssembly, Settings);

			Duplicates.Add(resource);
			return resource;
		}

		public override void Dispose(bool reloading) {
			Texture?.Dispose();
			base.Dispose(reloading);
		}

		public static implicit operator Texture(TextureResource resource) => resource.Texture;
		
		public class Import : ImportMetadata {

			public override string FormatId => "texture";

			public Texture.TextureFilter Filter { get; set; } = Texture.TextureFilter.Linear;
			public Texture.TextureWrapMode WrapMode { get; set; } = Texture.TextureWrapMode.Repeat;
			public Texture.TextureFormat Format { get; set; } = Texture.TextureFormat.RGBA8;
		}
	}
}
