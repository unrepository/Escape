namespace GENESIS.GPU {
	
	public interface Platform : IDisposable {
		
		public bool IsInitialized { get; protected set; }
		
		public void Initialize();
	}
}
