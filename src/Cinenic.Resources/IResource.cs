using Cinenic.Renderer;

namespace Cinenic.Resources {
	
	public interface IResource<TImportSettings> : IResource
		where TImportSettings : ImportSettings, new()
	{
		public new TImportSettings Settings { get; }

		void Load(IPlatform platform, string filePath, Stream stream, TImportSettings? settings);

		public void Save() {
			// TODO
		}
	}

	public interface IResource : IRefCounted {
		
		public string FilePath { get; }
		
		public Guid Id { get; }
		public ImportSettings Settings { get; }
		
		abstract static Type SettingsType { get; }

		void Load(IPlatform platform, string filePath, Stream stream, ImportSettings? settings);
	}
}
