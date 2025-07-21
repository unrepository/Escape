using System.Text.Json;
using Cinenic.Renderer;
using NLog;

namespace Cinenic.Resources {
	
	public abstract class Resource<TImportSettings> : IResource
		where TImportSettings : ImportSettings, new()
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		public uint ReferenceCount { get; set; }
		public bool IsValidObject { get; set; }
		public event IRefCounted.FreedEventHandler? Freed;
		
		public delegate void ReloadedEventHandler(Resource<TImportSettings> sender);
		public event ReloadedEventHandler? Reloaded;

		public abstract Type SettingsType { get; }

		public IPlatform Platform { get; protected set; }
		public string? FilePath { get; protected set; }
		
		public Guid Id { get; protected set; }
		public TImportSettings Settings { get; protected set; }
		
		public void Load(IPlatform platform, string? filePath, byte[] data, ImportSettings? settings) {
			using var stream = new MemoryStream(data);
			Load(platform, filePath, stream, settings as TImportSettings);
		}
		
		public void Load(IPlatform platform, string? filePath, Stream stream, ImportSettings? settings) {
			Load(platform, filePath, stream, settings as TImportSettings);
		}

		public virtual void Load(IPlatform platform, string? filePath, Stream stream, TImportSettings? settings) {
			settings ??= new();

			Platform = platform;
			FilePath = filePath;
			Id = settings.Id;
			Settings = settings;
		}

		public virtual void Save() {
			throw new NotImplementedException();
		}
		
		public virtual void Reload() {
			if(FilePath is null) return;
			
			_logger.Trace("{Path} reload initiated", FilePath);
			
			if(!File.Exists(FilePath)) {
				_logger.Warn("{Path} no longer exists, cannot reload", FilePath);
				return;
			}
			
			Dispose();
			
			TImportSettings? importSettings = null;

			if(File.Exists(FilePath + ImportSettings.FileExtension)) {
				using var importStream = new FileStream(FilePath + ImportSettings.FileExtension, FileMode.Open);
				importSettings = JsonSerializer.Deserialize<TImportSettings>(importStream, ImportSettings.DefaultSerializerOptions);
			}

			importSettings ??= new();
			using var fileStream = new FileStream(FilePath, FileMode.Open);
			Load(Platform, FilePath, fileStream, importSettings);
			
			Reloaded?.Invoke(this);
			
			_logger.Trace("Finished reloading resource {Path}", FilePath);
		}

		public virtual void Dispose() {
			Freed?.Invoke(this);
			GC.SuppressFinalize(this);
		}
	}

	public interface IResource : IRefCounted, IReloadable {
		
		public Type SettingsType { get; }
		
		public IPlatform Platform { get; }
		public string FilePath { get; }
		
		public Guid Id { get; }

		void Load(IPlatform platform, string filePath, Stream stream, ImportSettings? settings);
	}
}
