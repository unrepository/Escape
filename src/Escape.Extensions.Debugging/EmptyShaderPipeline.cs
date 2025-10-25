using System.Numerics;
using Escape.Renderer;
using Escape.Renderer.Camera;
using Escape.Renderer.Lights;
using Escape.Renderer.Resources;
using Escape.Renderer.Shader;
using Escape.Resources;
using Silk.NET.Vulkan;

namespace Escape.Extensions.Debugging {
	
	internal class EmptyShaderPipeline : IShaderPipeline {
		
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

		public DescriptorSet VkTexturesDescriptor { get; }
		
		public int GLModelMatrixUniform { get; }
		public int GLAvailableTexturesUniform { get; }

		public EmptyShaderPipeline(IPlatform platform) {
			Platform = platform;
			Program = ResourceManager.Load<ShaderProgramResource>(platform, "/shader_programs/empty.program")!;
		}
		
		public void PushData() { }
		
		public void Dispose() {
			GC.SuppressFinalize(this);
			Program.Dispose();
		}
	}
}
