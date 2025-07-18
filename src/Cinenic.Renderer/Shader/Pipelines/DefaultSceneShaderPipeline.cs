using System.Numerics;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer.Camera;
using Cinenic.Renderer.Vulkan;

namespace Cinenic.Renderer.Shader.Pipelines {
	
	public class DefaultSceneShaderPipeline : IShaderPipeline {
		
		public ShaderProgram Program { get; private set; }

		public IShaderData<CameraData> CameraData;
		public IShaderArrayData<Vertex> VertexData;
		public IShaderArrayData<uint> IndexData;
		public IShaderData<Material> MaterialData;
		public IShaderData<Matrix4x4> MatrixData;
		
		public DefaultSceneShaderPipeline(IPlatform platform) {
			Shader[] shaders = platform switch {
				VkPlatform vkPlatform => [
					new VkShader(vkPlatform, Shader.Family.Vertex, Resources.LoadText("Shaders.Vulkan.scene.vert")),
					new VkShader(vkPlatform, Shader.Family.Fragment, Resources.LoadText("Shaders.Vulkan.scene.frag")),
				],
				_ => throw new NotImplementedException("PlatformImpl")
			};
			
			Program = ShaderProgram.Create(platform, shaders);

			CameraData = IShaderData.Create<CameraData>(platform, Program, 0, default);
			VertexData = IShaderArrayData.Create<Vertex>(platform, Program, 1, null, 16);
			IndexData = IShaderArrayData.Create<uint>(platform, Program, 2, null, 16);
			MaterialData = IShaderData.Create<Material>(platform, Program, 3, default);
			MatrixData = IShaderData.Create<Matrix4x4>(platform, Program, 4, default);
		}

		public void PushData() {
			CameraData.Push();
			VertexData.Push();
			IndexData.Push();
			MaterialData.Push();
			MatrixData.Push();
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			
			CameraData.Dispose();
			VertexData.Dispose();
			IndexData.Dispose();
			MaterialData.Dispose();
			MatrixData.Dispose();
			
			Program.Dispose();
		}
	}
}
