using System.Numerics;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer.Camera;
using Cinenic.Renderer.OpenGL;
using Cinenic.Renderer.Resources;
using Cinenic.Renderer.Vulkan;
using Cinenic.Resources;
using Silk.NET.Vulkan;

namespace Cinenic.Renderer.Shader.Pipelines {
	
	public class DefaultSceneShaderPipeline : IShaderPipeline {

		public IPlatform Platform { get; }
		public Ref<ShaderProgramResource> Program { get; }

		public IShaderData<CameraData> CameraData;
		public IShaderArrayData<Vertex> VertexData;
		public IShaderArrayData<uint> IndexData;
		public IShaderArrayData<Material.Data> MaterialData;
		public IShaderArrayData<Matrix4x4> MatrixData;

	#region Vulkan
		public DescriptorSet VkTexturesDescriptor { get; }
		private DescriptorSet _textureDescriptorSet;
	#endregion
		
		public DefaultSceneShaderPipeline(IPlatform platform) {
			Platform = platform;
			
			var vkPlatform = platform as VkPlatform;
			
			// Shader[] shaders = platform switch {
			// 	VkPlatform => [
			// 		new VkShader(vkPlatform!, Shader.Family.Vertex, Extensions.CSharp.Resources.LoadText("Shaders.Vulkan.scene.vert")),
			// 		new VkShader(vkPlatform!, Shader.Family.Fragment, Extensions.CSharp.Resources.LoadText("Shaders.Vulkan.scene.frag")),
			// 	],
			// 	_ => throw new NotImplementedException("PlatformImpl")
			// };

			Program = ResourceManager.Load<ShaderProgramResource>(platform, "/shader_programs/scene.program")!;
			
			//Program = ShaderProgram.Create(platform, shaders);
			
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

				// var textureDescriptor = VkHelpers.CreateDescriptorSet(
				// 	vkPlatform,
				// 	vkProgram,
				// 	[
				// 		(uint) Material.TextureType.Albedo,
				// 		(uint) Material.TextureType.Normal,
				// 		(uint) Material.TextureType.Metallic,
				// 		(uint) Material.TextureType.Roughness
				// 	],
				// 	1,
				// 	DescriptorType.CombinedImageSampler,
				// 	ShaderStageFlags.FragmentBit
				// );
				//
				// _textureDescriptorSet = textureDescriptor.Set;
			}
		#endregion

			// SSBOs
			// TODO *technically* everything should be Ref, as it might get cleaned up by GC in scenario where Program
			// was would be a local variable, but that seems kinda eh, maybe in the future
			CameraData = IShaderData.Create<CameraData>(platform, Program.Get(), 0, default);
			VertexData = IShaderArrayData.Create<Vertex>(platform, Program.Get(), 1, null, 1024);
			IndexData = IShaderArrayData.Create<uint>(platform, Program.Get(), 2, null, 1024);
			MaterialData = IShaderArrayData.Create<Material.Data>(platform, Program.Get(), 3, null, 1024);
			MatrixData = IShaderArrayData.Create<Matrix4x4>(platform, Program.Get(), 4, null, 1024);
		}

		public void PushData() {
			CameraData.Push();
			VertexData.Push();
			IndexData.Push();
			MaterialData.Push();
			MatrixData.Push();
		}

	#region Vulkan
		[Obsolete]
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
