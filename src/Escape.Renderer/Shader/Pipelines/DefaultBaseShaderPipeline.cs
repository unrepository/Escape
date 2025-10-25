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
	
	public class DefaultBaseShaderPipeline : IShaderPipeline {

		public IPlatform Platform { get; }
		public Ref<ShaderProgramResource> Program { get; }

		public IShaderData<CameraData> CameraData { get; protected set; }
		
		public IShaderArrayData<Vertex> VertexData { get; protected set; }
		public IShaderArrayData<uint> IndexData { get; protected set; }
		public IShaderArrayData<Material.Data> MaterialData { get; protected set; }
		public IShaderArrayData<Matrix4x4> MatrixData { get; protected set; }

		public IShaderData<LightData> LightData { get; protected set; }
		public IShaderArrayData<DirectionalLight> DirectionalLightData { get; protected set; }
		public IShaderArrayData<PointLight> PointLightData { get; protected set; }
		public IShaderArrayData<SpotLight> SpotLightData { get; protected set; }
		
	#region Vulkan
		public DescriptorSet VkTexturesDescriptor { get; protected set; }
		private DescriptorSet _textureDescriptorSet;
	#endregion

	#region OpenGL
		public int GLModelMatrixUniform { get; protected set; }
		public int GLAvailableTexturesUniform { get; protected set; }
	#endregion
		
		public DefaultBaseShaderPipeline(IPlatform platform, Ref<ShaderProgramResource> shaderProgram) {
			Platform = platform;
			
			var vkPlatform = platform as VkPlatform;
			var glPlatform = platform as GLPlatform;

			Program = shaderProgram;
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
				var pHandle = Program.Get().Program.Handle;
				
				GLModelMatrixUniform = glPlatform.API.GetUniformLocation(pHandle, "modelMatrix");
				GLAvailableTexturesUniform = glPlatform.API.GetUniformLocation(pHandle, "availableTextures");
			}
		#endregion

			CreateData();
		}

		protected void CreateData() {
			// TODO *technically* everything should be Ref, as it might get cleaned up by GC in scenario where Program
			// was would be a local variable, but that seems kinda eh, maybe in the future
			CameraData = IShaderData.Create<CameraData>(Platform, Program.Get(), "CameraData", 0, default);

			const int INITIAL_SIZE = 1024 * 1024; // 1 MiB to start

			// we don't use PVP in OpenGL
			if(Platform is not GLPlatform) {
				VertexData = IShaderArrayData.Create<Vertex>(Platform, Program.Get(), "VertexData", 1, null, INITIAL_SIZE);
				IndexData = IShaderArrayData.Create<uint>(Platform, Program.Get(), "IndexData", 2, null, INITIAL_SIZE);
				MatrixData = IShaderArrayData.Create<Matrix4x4>(Platform, Program.Get(), "MatrixData", 4, null, INITIAL_SIZE);
			}
 
			MaterialData = IShaderArrayData.Create<Material.Data>(Platform, Program.Get(), "MaterialData", 3, null, INITIAL_SIZE);
			
			LightData = IShaderData.Create<LightData>(Platform, Program.Get(), "LightData", 10, default);
			DirectionalLightData = IShaderArrayData.Create<DirectionalLight>(Platform, Program.Get(), "DirectionalLightData", 11, null, 64);
			PointLightData = IShaderArrayData.Create<PointLight>(Platform, Program.Get(), "PointLightData", 12, null, 64);
			SpotLightData = IShaderArrayData.Create<SpotLight>(Platform, Program.Get(), "SpotLightData", 13, null, 64);
		}

		public void PushData() {
			CameraData.Push();

			VertexData.Push();
			IndexData.Push();
			MatrixData.Push();
			
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
