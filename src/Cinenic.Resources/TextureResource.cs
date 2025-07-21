using Cinenic.Renderer;

namespace Cinenic.Resources {
	
	public class TextureResource : IResource<TextureResource.Import> {

		public static Type SettingsType => typeof(Import);

		public string FilePath { get; private set; }

		public Guid Id { get; private set; }
		public Import Settings { get; private set; }
		ImportSettings IResource.Settings => Settings;
		
		public Texture? Texture { get; private set; }
		
		public void Load(IPlatform platform, string filePath, Stream stream, ImportSettings? settings) {
			Load(platform, filePath, stream, settings as Import);
		}
		
		public void Load(IPlatform platform, string filePath, Stream stream, Import? settings) {
			settings ??= new();

			FilePath = filePath;
			Id = settings.Id;
			Settings = settings;

			Texture = Texture.Create(
				platform,
				stream,
				settings.Filter,
				settings.WrapMode,
				settings.Format
			);
		}

		public void Dispose() {
			Texture?.Dispose();
		}

		public static implicit operator Texture(TextureResource resource) => resource.Texture;
		
		public class Import : ImportSettings {

			public override string Type => "texture";

			public Texture.TextureFilter Filter { get; set; } = Texture.TextureFilter.Nearest;
			public Texture.TextureWrapMode WrapMode { get; set; } = Texture.TextureWrapMode.Repeat;
			public Texture.TextureFormat Format { get; set; } = Texture.TextureFormat.RGBA8;
		}
	}
}
