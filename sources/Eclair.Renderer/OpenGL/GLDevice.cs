namespace Eclair.Renderer.OpenGL {
	
	public class GLDevice : IDevice {

		public uint Index => throw new NotSupportedException();
		public string Name => throw new NotSupportedException();
		public bool Headless => throw new NotSupportedException();

		public void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
