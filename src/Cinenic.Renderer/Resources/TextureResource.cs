using System.Reflection;
using Cinenic.Renderer;
using Cinenic.Resources;

namespace Cinenic.Renderer.Resources {
	
	public class TextureResource : Resource<TextureResource.Import> {

		public override Type MetadataType => typeof(Import);
		public override string[] FileExtensions => [ ".png", ".jpg", ".bmp", ".jpeg", ".tiff", ".tga", ".webp", ".pbm", ".qoi" ];

		public Texture? Texture { get; private set; }

		public void Create(Texture texture) {
			Platform = texture.Platform;
			Settings = new Import();
			Id = Settings.Id;
			
			Texture = texture;
		}
		
		public override void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, Import? settings) {
			base.Load(platform, filePath, stream, resourceAssembly, settings);
			
			Texture = Texture.Create(
				platform,
				stream,
				Settings.Filter,
				Settings.WrapMode,
				Settings.Format
			);
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
