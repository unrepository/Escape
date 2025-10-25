using System.Reflection;
using System.Text.Json;
using NLog;
using Escape.Renderer;

namespace Escape.Resources {
	
	public abstract class Resource<TImportSettings> : IResource
		where TImportSettings : ImportMetadata, new()
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		public uint ReferenceCount { get; set; }
		public bool IsValidObject { get; set; }
		public event IRefCounted.FreedEventHandler? Freed;
		
		public delegate void ReloadedEventHandler(Resource<TImportSettings> sender);
		public event ReloadedEventHandler? Reloaded;

		public abstract Type MetadataType { get; }
		public abstract string[] FileExtensions { get; }

		public IPlatform Platform { get; protected set; }
		public string? FilePath { get; protected set; }
		public Assembly ResourceAssembly { get; protected set; }

		public Guid Id { get; protected set; }
		public TImportSettings Settings { get; protected set; }
		public List<IResource> Dependencies { get; protected set; } = [];
		
		// ~Resource() {
		// 	Dispose(false);
		// }
		
		public void Load(IPlatform platform, string filePath, byte[] data, Assembly resourceAssembly, ImportMetadata? settings) {
			using var stream = new MemoryStream(data);
			Load(platform, filePath, stream, resourceAssembly, settings as TImportSettings);
		}
		
		public void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, ImportMetadata? settings) {
			Load(platform, filePath, stream, resourceAssembly, settings as TImportSettings);
		}

		public virtual void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, TImportSettings? settings) {
			settings ??= new();
			
			Platform = platform;
			FilePath = filePath;
			ResourceAssembly = resourceAssembly;
			
			Id = settings.Id;
			Settings = settings;
			Dependencies = [];
		}

		public virtual bool Save() {
			if(FilePath is null) return false;
			Settings.Save(typeof(TImportSettings));
			
			_logger.Debug(
				"Saved {Type} import settings to {Path}",
				GetType().Name, Settings.Path
			);
			
			return true;
		}
		
		public virtual bool Reload() {
			if(FilePath is null) return false;
			
			_logger.Trace("{Path} reload initiated", FilePath);
			
			if(!File.Exists(FilePath)) {
				_logger.Warn("{Path} no longer exists, cannot reload", FilePath);
				return false;
			}
			
			//GC.SuppressFinalize(this);
			Dispose(true);
			
			TImportSettings? importSettings = null;

			if(File.Exists(FilePath + ImportMetadata.FILE_EXTENSION)) {
				using var importStream = new FileStream(FilePath + ImportMetadata.FILE_EXTENSION, FileMode.Open);
				importSettings = JsonSerializer.Deserialize<TImportSettings>(importStream, ImportMetadata.DefaultSerializerOptions);
			}

			importSettings ??= new();
			using var fileStream = new FileStream(FilePath, FileMode.Open);
			Load(Platform, FilePath, fileStream, ResourceAssembly, importSettings);
			
			Reloaded?.Invoke(this);
			
			_logger.Debug("Finished reloading resource {Path}", FilePath);
			return true;
		}

		public void Dispose() {
			GC.SuppressFinalize(this);

			try {
				Dispose(false);
			} catch(Exception e) {
				_logger.Error("Could not dispose resource {Id} ({Path}): {Exception}", Id, FilePath, e);
			}
		}
		
		public virtual void Dispose(bool reloading) {
			if(!reloading) {
				// implicitly save only when debugging, as it will be normally done through the resource editor
			#if DEBUG
				Save();
			#endif
				Freed?.Invoke(this);
			}
		}
		
		public static implicit operator Resource<TImportSettings>(Ref<Resource<TImportSettings>> resource) => resource.Get();
	}

	public interface IResource : IRefCounted, IReloadable {
		
		public Type MetadataType { get; }
		public string[] FileExtensions { get; }
		
		public IPlatform Platform { get; }
		public string FilePath { get; }
		public Assembly ResourceAssembly { get; }
		
		public Guid Id { get; }
		public List<IResource> Dependencies { get; }

		void Load(IPlatform platform, string filePath, Stream stream, Assembly resourceAssembly, ImportMetadata? settings);
	}
}
