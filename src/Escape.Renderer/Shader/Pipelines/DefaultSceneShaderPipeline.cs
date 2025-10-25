using System.Numerics;
using Escape.Renderer.Camera;
using Escape.Renderer.Lights;
using Escape.Renderer.Resources;
using Escape.Renderer.Vulkan;
using Escape.Extensions.CSharp;
using Escape.Renderer.OpenGL;
using Escape.Resources;
using Silk.NET.Vulkan;

namespace Escape.Renderer.Shader.Pipelines {
	
	public class DefaultSceneShaderPipeline : IShaderPipeline {

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
		
	#region Vulkan
		public DescriptorSet VkTexturesDescriptor { get; }
		private DescriptorSet _textureDescriptorSet;
	#endregion

	#region OpenGL
		public int GLModelMatrixUniform { get; }
	#endregion
		
		public DefaultSceneShaderPipeline(IPlatform platform) {
			Platform = platform;
			
			var vkPlatform = platform as VkPlatform;
			var glPlatform = platform as GLPlatform;

			Program = ResourceManager.Load<ShaderProgramResource>(platform, "/shader_programs/scene.program")!;
			Program.Get().Program.Build();
			
		#region Vulkan
			// material texture units for Vulkan
			if(vkPlatform is not null) {
				var vkProgram = (VkShaderProgram) Program.Get();
				
				var textureDescriptor = VkHelpers.CreateDescriptorSet(
					vkPlatform,
					vkProgram,
					[0],
					1024,
					DescriptorType.CombinedImageSampler,
					ShaderStageFlags.FragmentBit
				);

				VkTexturesDescriptor = textureDescriptor.Set;
			}
		#endregion

		#region OpenGL
			if(glPlatform is not null) {
				GLModelMatrixUniform = glPlatform.API.GetUniformLocation(Program.Get().Program.Handle, "model_matrix");
			}
		#endregion

			// shader data
			// TODO *technically* everything should be Ref, as it might get cleaned up by GC in scenario where Program
			// was would be a local variable, but that seems kinda eh, maybe in the future
			CameraData = IShaderData.Create<CameraData>(platform, Program.Get(), "CameraData", 0, default);

			const int INITIAL_SIZE = 1024 * 1024; // 1 MiB to start
			
			VertexData = IShaderArrayData.Create<Vertex>(platform, Program.Get(), "VertexData", 1, null, INITIAL_SIZE);
			IndexData = IShaderArrayData.Create<uint>(platform, Program.Get(), "IndexData", 2, null, INITIAL_SIZE);
			MaterialData = IShaderArrayData.Create<Material.Data>(platform, Program.Get(), "MaterialData", 3, null, INITIAL_SIZE);
			MatrixData = IShaderArrayData.Create<Matrix4x4>(platform, Program.Get(), "MatrixData", 4, null, INITIAL_SIZE);
 
			LightData = IShaderData.Create<LightData>(platform, Program.Get(), "LightData", 10, default);
			DirectionalLightData = IShaderArrayData.Create<DirectionalLight>(platform, Program.Get(), "DirectionalLightData", 11, null, 64);
			PointLightData = IShaderArrayData.Create<PointLight>(platform, Program.Get(), "PointLightData", 12, null, 64);
			SpotLightData = IShaderArrayData.Create<SpotLight>(platform, Program.Get(), "SpotLightData", 13, null, 64);
		}

		public void PushData() {
			CameraData.Push();

			// we don't use PVP in OpenGL
			if(Platform is not GLPlatform) {
				VertexData.Push();
				IndexData.Push();
				MatrixData.Push();
			}
			
			MaterialData.Push();
			
			LightData.Push();
			DirectionalLightData.Push();
			PointLightData.Push();
			SpotLightData.Push();
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
