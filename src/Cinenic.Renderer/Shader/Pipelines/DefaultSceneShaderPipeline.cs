using System.Numerics;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer.Camera;
using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Vulkan;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Shader.Pipelines {
	
	public class DefaultSceneShaderPipeline : IShaderPipeline {

		public IPlatform Platform { get; }
		public ShaderProgram Program { get; }

		public IShaderData<CameraData> CameraData;
		public IShaderArrayData<Vertex> VertexData;
		public IShaderArrayData<uint> IndexData;
		public IShaderData<Material.Data> MaterialData;
		public IShaderData<Matrix4x4> MatrixData;

	#region Vulkan
		private DescriptorSet _textureDescriptorSet;
	#endregion
		
		public DefaultSceneShaderPipeline(IPlatform platform) {
			Platform = platform;
			
			var vkPlatform = platform as VkPlatform;
			
			Shader[] shaders = platform switch {
				VkPlatform => [
					new VkShader(vkPlatform!, Shader.Family.Vertex, Resources.LoadText("Shaders.Vulkan.scene.vert")),
					new VkShader(vkPlatform!, Shader.Family.Fragment, Resources.LoadText("Shaders.Vulkan.scene.frag")),
				],
				_ => throw new NotImplementedException("PlatformImpl")
			};
			
			Program = ShaderProgram.Create(platform, shaders);
			
		#region Vulkan
			// material texture units for Vulkan
			if(vkPlatform is not null) {
				var vkProgram = (VkShaderProgram) Program;
				
				var textureDescriptor = VkHelpers.CreateDescriptorSet(
					vkPlatform,
					vkProgram,
					[
						(uint) Material.TextureType.Albedo,
						(uint) Material.TextureType.Normal,
						(uint) Material.TextureType.Metallic,
						(uint) Material.TextureType.Roughness
					],
					DescriptorType.CombinedImageSampler,
					ShaderStageFlags.FragmentBit
				);

				_textureDescriptorSet = textureDescriptor.Set;
			}
		#endregion

			// SSBOs
			CameraData = IShaderData.Create<CameraData>(platform, Program, 0, default);
			VertexData = IShaderArrayData.Create<Vertex>(platform, Program, 1, null, 16);
			IndexData = IShaderArrayData.Create<uint>(platform, Program, 2, null, 16);
			MaterialData = IShaderData.Create<Material.Data>(platform, Program, 3, default);
			MatrixData = IShaderData.Create<Matrix4x4>(platform, Program, 4, default);
		}

		public void PushData() {
			CameraData.Push();
			VertexData.Push();
			IndexData.Push();
			MaterialData.Push();
			MatrixData.Push();
		}

	#region Vulkan
		public unsafe void VkBindTextureUnit(uint unit, ImageView imageView, Sampler sampler) {
			var platform = (VkPlatform) Platform;
			var device = platform.PrimaryDevice.Logical;
					
			var descriptorImageInfo = new DescriptorImageInfo {
				ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
				ImageView = imageView,
				Sampler = sampler
			};

			var writeDescriptorSet = new WriteDescriptorSet {
				SType = StructureType.WriteDescriptorSet,
				DstSet = _textureDescriptorSet,
				DstBinding = unit,
				DstArrayElement = 0, // TODO what is this
				DescriptorType = DescriptorType.CombinedImageSampler,
				DescriptorCount = 1,
				PImageInfo = &descriptorImageInfo
			};
			
			platform.API.UpdateDescriptorSets(device, 1, writeDescriptorSet, 0, null);
		}
	#endregion

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
