namespace Visio.Renderer {
	
	public interface IPlatform : IDisposable {
		
		public Platform Identifier { get; }
		
		public Thread PlatformThread { get; protected set; }
		public bool IsInitialized { get; protected set; }
		
		public void Initialize();
	}
}
