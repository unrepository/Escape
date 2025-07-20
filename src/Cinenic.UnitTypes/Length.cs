using System.Numerics;

namespace Cinenic.UnitTypes {
	
	// TODO generic
	public struct Length {

		public double AU {
			get => Meters / 149_597_870_700.0;
			set => Meters = value * 149_597_870_700.0;
		}

		public double Kilometers {
			get => Meters / 1000.0;
			set => Meters = value * 1000.0;
		}
		
		public double Meters { get; set; }

		public double Centimeters {
			get => Meters * 100.0;
			set => Meters = value / 100.0;
		}

		public double SI => Meters;

		public static Length operator +(Length a, Length b) => new() { Meters = a.Meters + b.Meters };
		public static Length operator -(Length a, Length b) => new() { Meters = a.Meters - b.Meters };
		
		public static Length operator *(Length a, int b) => new() { Meters = a.Meters * b };
		public static Length operator *(Length a, double b) => new() { Meters = a.Meters * b };
		public static Length operator /(Length a, int b) => new() { Meters = a.Meters / b };
		public static Length operator /(Length a, double b) => new() { Meters = a.Meters / b };
		
		public static bool operator ==(Length a, Length b) => a.Equals(b);
		public static bool operator !=(Length a, Length b) => !a.Equals(b);
		
		public bool Equals(Length a) => Math.Abs(Meters - a.Meters) < 0.001;
		public override bool Equals(object? obj) => obj is Length other && Equals(other);

		public override string ToString() => $"{Meters} m";

		public static Length FromAU(double value) => new() { AU = value };
		public static Length FromKilometers(double value) => new() { Kilometers = value };
		public static Length FromMeters(double value) => new() { Meters = value };
		public static Length FromCentimeters(double value) => new() { Centimeters = value };
	}
}
