namespace Cinenic.Renderer {
	
	public interface IPlatform : IDisposable {
		
		public bool IsInitialized { get; protected set; }
		
		public void Initialize();
	}
}
