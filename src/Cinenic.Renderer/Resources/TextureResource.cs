using Cinenic.Renderer;
using Cinenic.Resources;

namespace Cinenic.Renderer.Resources {
	
	public class TextureResource : Resource<TextureResource.Import> {

		static TextureResource() {
			ResourceRegistry.RegisterFormat<TextureResource, Import>();
		}

		public override Type SettingsType => typeof(Import);
		
		public Texture? Texture { get; private set; }

		public override void Load(IPlatform platform, string filePath, Stream stream, Import? settings) {
			base.Load(platform, filePath, stream, settings);
			
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
		public static implicit operator TextureResource(Ref<TextureResource> resource) => resource.Get();
		
		public class Import : ImportSettings {

			public override string Type => "texture";

			public Texture.TextureFilter Filter { get; set; } = Texture.TextureFilter.Nearest;
			public Texture.TextureWrapMode WrapMode { get; set; } = Texture.TextureWrapMode.Repeat;
			public Texture.TextureFormat Format { get; set; } = Texture.TextureFormat.RGBA8;
		}
	}
}
