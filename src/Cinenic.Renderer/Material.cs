using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer.Resources;
using Cinenic.Resources;

namespace Cinenic.Renderer {
	
	public class Material : ITypeCloneable<Material> {

		public Color AlbedoColor = Color.White;
		public float Roughness = 0.5f;
		public float Metallic = 0.0f;
		public float IOR = 1.5f;

		public Ref<TextureResource>? AlbedoTexture;
		public Ref<TextureResource>? NormalTexture;
		public Ref<TextureResource>? MetallicTexture;
		public Ref<TextureResource>? RoughnessTexture;
		
		public static implicit operator Material(Color color)
			=> new Material { AlbedoColor = color };

		public Data CreateData() {
			var data = new Data {
				AlbedoColor = AlbedoColor.ToVector4(),
				Roughness = Roughness,
				Metallic = Metallic,
				IOR = IOR
			};

			// if(AlbedoTexture is not null && AlbedoTexture.IsValid) data.UseTextures |= TextureType.Albedo;
			// if(NormalTexture is not null && NormalTexture.IsValid) data.UseTextures |= TextureType.Normal;
			// if(MetallicTexture is not null && MetallicTexture.IsValid) data.UseTextures |= TextureType.Metallic;
			// if(RoughnessTexture is not null && RoughnessTexture.IsValid) data.UseTextures |= TextureType.Roughness;
			
			return data;
		}

		public Material Clone() {
			return new() {
				AlbedoColor = AlbedoColor,
				Roughness = Roughness,
				Metallic = Metallic,
				AlbedoTexture = AlbedoTexture,
				MetallicTexture = MetallicTexture,
				NormalTexture = NormalTexture,
				RoughnessTexture = RoughnessTexture
			};
		}
		
		public override string ToString() {
			return 
				$"[Albedo={AlbedoColor}, Roughness={Roughness}, Metallic={Metallic}, "
				+ $"AlbedoTexture={AlbedoTexture?.Get().Id}, NormalTexture={NormalTexture?.Get().Id}, "
				+ $"RoughnessTexture={RoughnessTexture?.Get().Id}, MetallicTexture={MetallicTexture?.Get().Id}]";
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Data {
			
			[FieldOffset(0)] public Vector4 AlbedoColor;
			[FieldOffset(16)] public float Roughness;
			[FieldOffset(20)] public float Metallic;
			[FieldOffset(24)] public float IOR;
			//[FieldOffset(24)] public TextureType UseTextures;
			[FieldOffset(28)] private float _padding0;
		}

		/*[Flags]
		public enum TextureType : uint {
			
			None = 0,
			Albedo = (1 << 0),
			Normal = (1 << 1),
			Metallic = (1 << 2),
			Roughness = (1 << 3),
		}*/

		/*public static Material Create(Color albedoColor, float metallic = 0.5f, float roughness = 0.5f)
			=> new Material {
				Albedo = albedoColor.ToVector4(),
				Metallic = metallic,
				Roughness = roughness,
				UseTextures = 0
			};

		public static Material Create(
			Texture diffuseTexture,
			Texture? normalTexture = null,
			Texture? roughnessTexture = null,
			Texture? metallicTexture = null,
			Color? albedoColor = null,
			float metallic = 0.5f,
			float roughness = 0.5f
		) {
			albedoColor ??= Color.White;

			var material = new Material {
				Albedo = albedoColor.Value.ToVector4(),
				Metallic = metallic,
				Roughness = roughness
			};

			material.UseTextures |= (1 << 0);
			if(normalTexture is not null) material.UseTextures |= (1 << 1);
			if(roughnessTexture is not null) material.UseTextures |= (1 << 2);
			if(metallicTexture is not null) material.UseTextures |= (1 << 3);

			return material;
		}*/
	}
}
