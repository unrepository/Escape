namespace Cinenic.Renderer {
	
	public interface IPlatform : IDisposable {
		
		public Thread PlatformThread { get; protected set; }
		public bool IsInitialized { get; protected set; }
		
		public void Initialize();
	}
}
