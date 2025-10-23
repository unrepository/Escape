using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Escape.Renderer.Resources;
using Escape.Extensions.CSharp;
using Escape.Resources;

namespace Escape.Renderer {
	
	public class Material : ITypeCloneable<Material> {

		public Color AlbedoColor = Color.White;
		public float Roughness = 0.5f;
		public float Metallic = 0.0f;
		public float IOR = 1.5f;

		public bool ComplexParallaxMapping = false;
		public uint ParallaxMinLayers = 4;
		public uint ParallaxMaxLayers = 32;
		public float ParallaxHeightScale = -0.05f;

		public Ref<TextureResource>? AlbedoTexture;
		public Ref<TextureResource>? NormalTexture;
		public Ref<TextureResource>? MetallicTexture;
		public Ref<TextureResource>? RoughnessTexture;
		public Ref<TextureResource>? HeightTexture;
		
		public static implicit operator Material(Color color)
			=> new Material { AlbedoColor = color };

		public Data CreateData() {
			var data = new Data {
				AlbedoColor = AlbedoColor,
				Roughness = Roughness,
				Metallic = Metallic,
				IOR = IOR,
				PMComplex = ComplexParallaxMapping,
				PMMinLayers = ParallaxMinLayers,
				PMMaxLayers = ParallaxMaxLayers,
				PMHeightScale = ParallaxHeightScale
			};

			return data;
		}

		public Material Clone() {
			return new() {
				AlbedoColor = AlbedoColor,
				Roughness = Roughness,
				Metallic = Metallic,
				IOR = IOR,
				ComplexParallaxMapping = ComplexParallaxMapping,
				ParallaxMinLayers = ParallaxMinLayers,
				ParallaxMaxLayers = ParallaxMaxLayers,
				ParallaxHeightScale = ParallaxHeightScale,
				AlbedoTexture = AlbedoTexture,
				MetallicTexture = MetallicTexture,
				NormalTexture = NormalTexture,
				RoughnessTexture = RoughnessTexture,
				HeightTexture = HeightTexture
			};
		}
		
		public override string ToString() {
			return 
				$"[Albedo={AlbedoColor}, Roughness={Roughness}, Metallic={Metallic}, IOR={IOR}, "
				+ $"AlbedoTexture={AlbedoTexture?.Get().Id}, NormalTexture={NormalTexture?.Get().Id}, "
				+ $"RoughnessTexture={RoughnessTexture?.Get().Id}, MetallicTexture={MetallicTexture?.Get().Id}, "
				+ $"DisplacementTexture={HeightTexture?.Get().Id}]";
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Data {
			
			[FieldOffset(0)] public Vector4 AlbedoColor;
			[FieldOffset(16)] public float Roughness;
			[FieldOffset(20)] public float Metallic;
			[FieldOffset(24)] public float IOR;
			[FieldOffset(28)] public bool PMComplex;
			[FieldOffset(32)] public uint PMMinLayers;
			[FieldOffset(36)] public uint PMMaxLayers;
			[FieldOffset(40)] public float PMHeightScale;
			[FieldOffset(44)] private float _padding0;
		}
	}
}
