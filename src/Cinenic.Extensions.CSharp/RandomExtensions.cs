using System.Numerics;

namespace Cinenic.Extensions.CSharp {
	
	public static class RandomExtensions {
		
		public static Vector3 NextSphereCoordinate(this Random random, double radius) {
			// Generate random angles and radius
			double theta = 2 * Math.PI * random.NextDouble(); // Azimuthal angle
			double phi = Math.Acos(2 * random.NextDouble() - 1); // Polar angle
			double r = radius * Math.Pow(random.NextDouble(), 1.0 / 3.0); // Radius with cubic root for uniform distribution

			// Convert to Cartesian coordinates
			double x = r * Math.Sin(phi) * Math.Cos(theta);
			double y = r * Math.Sin(phi) * Math.Sin(theta);
			double z = r * Math.Cos(phi);

			return new Vector3((float) x, (float) y, (float) z);
		}
	}
}
