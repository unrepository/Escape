namespace Visio.Renderer {
	
	public interface IDevice : IDisposable {
		
		public uint Index { get; }
		public string Name { get; }
		
		public bool Headless { get; }

		public void Initialize();
	}
}
