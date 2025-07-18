namespace Cinenic.Renderer.Shader {
	
	public interface IShaderPipeline : IDisposable {

		public ShaderProgram Program { get; }

		public void PushData();
	}
}
