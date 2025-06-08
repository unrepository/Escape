using GENESIS.GPU;

namespace GENESIS.Project {
	
	public interface IProjectObject {

		public FileInfo File { get; }
		
		public string Ext { get; }
		public static abstract string Extension { get; }

		public void Open(IPlatform platform, Window window);

		public void OpenExternal() {
			throw new NotImplementedException();
		}

		public static IProjectObject CreateFromFile(FileInfo file) {
			if(file.Extension == MapObject.Extension) return new MapObject(file);
			throw new NotImplementedException();
		}
	}
}
