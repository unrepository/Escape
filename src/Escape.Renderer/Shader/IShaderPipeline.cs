using System.Numerics;
using Escape.Renderer.Camera;
using Escape.Renderer.Lights;
using Escape.Renderer.Resources;
using Escape.Renderer.Vulkan;
using Escape.Resources;
using Silk.NET.Vulkan;

namespace Escape.Renderer.Shader {
	
	public interface IShaderPipeline : IDisposable {

		public IPlatform Platform { get; }
		public Ref<ShaderProgramResource> Program { get; }

		public IShaderData<CameraData> CameraData { get; }
		
		public IShaderArrayData<Vertex> VertexData { get; }
		public IShaderArrayData<uint> IndexData { get; }
		public IShaderArrayData<Material.Data> MaterialData { get; }
		public IShaderArrayData<Matrix4x4> MatrixData { get; }

		public IShaderData<LightData> LightData { get; }
		public IShaderArrayData<DirectionalLight> DirectionalLightData { get; }
		public IShaderArrayData<PointLight> PointLightData { get; }
		public IShaderArrayData<SpotLight> SpotLightData { get; }
		
		public void PushData();
		
	#region Vulkan
		public DescriptorSet VkTexturesDescriptor { get; }
	#endregion

	#region OpenGL
		public int GLModelMatrixUniform { get; }
		public int GLAvailableTexturesUniform { get; }
	#endregion
	}
}
