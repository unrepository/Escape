using Cinenic.Renderer.Shader;

namespace Cinenic.Renderer {
	
	public interface IPlatform : IDisposable {
		
		public bool IsInitialized { get; protected set; }
		public ShaderProgram? DefaultProgram { get; set; }
		
		public void Initialize();
	}
}
