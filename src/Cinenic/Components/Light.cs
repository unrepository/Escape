using Cinenic.Renderer;
using Cinenic.UnitTypes;

namespace Cinenic.Components {
	
	[Component]
	public record struct DirectionalLight(Color Color, float Intensity = 100) { }
	
	[Component]
	public record struct PointLight(Color Color, float Intensity = 100) { }

	[Component]
	public struct SpotLight {

		public Color Color;
		public float Intensity;
		
		public Rotation<float> Cutoff;
		public Rotation<float> CutoffOuter;

		public SpotLight(Color color, float intensity = 100, Rotation<float>? cutoff = null, Rotation<float>? cutoffOuter = null) {
			Color = color;
			Intensity = intensity;

			Cutoff = cutoff ?? Rotation<float>.FromDegrees(45);
			CutoffOuter = cutoffOuter ?? Rotation<float>.FromDegrees(2);
		}
	}
}
