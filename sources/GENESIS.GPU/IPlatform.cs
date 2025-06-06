namespace GENESIS.GPU {
	
	public interface IPlatform<TAPI, TDevice> : IDisposable
		where TDevice : IDevice {
		
		public bool IsInitialized { get; protected set; }
		public TDevice? PrimaryDevice { get; set; }
		
		public TAPI API { get; }
		
		public void Initialize();
		
		public IReadOnlyCollection<TDevice> GetDevices();
		public TDevice CreateDevice(int index);
	}
}
