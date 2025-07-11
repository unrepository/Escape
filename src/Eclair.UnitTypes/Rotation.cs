namespace Eclair.UnitTypes {
	
	public class Rotation {
		
		public double Degrees { get; set; }

		public double Radians {
			get => Degrees * (Math.PI / 180);
			set => Degrees = value * (180 / Math.PI);
		}

		public static Rotation operator +(Rotation a, Rotation b) => new() { Degrees = a.Degrees + b.Degrees };
		public static Rotation operator -(Rotation a, Rotation b) => new() { Degrees = a.Degrees - b.Degrees };
		
		public static Rotation operator *(Rotation a, int b) => new() { Degrees = a.Degrees * b };
		public static Rotation operator *(Rotation a, double b) => new() { Degrees = a.Degrees * b };
		public static Rotation operator /(Rotation a, int b) => new() { Degrees = a.Degrees / b };
		public static Rotation operator /(Rotation a, double b) => new() { Degrees = a.Degrees / b };
		
		public static bool operator ==(Rotation a, Rotation b) => a.Equals(b);
		public static bool operator !=(Rotation a, Rotation b) => !a.Equals(b);
		
		public bool Equals(Rotation a) => Math.Abs(Degrees - a.Degrees) < 0.001;
		public override bool Equals(object? obj) => obj is Rotation other && Equals(other);

		public override string ToString() => $"{Degrees}°";

		public static Rotation FromDegrees(double value) => new() { Degrees = value };
		public static Rotation FromRadians(double value) => new() { Radians = value };
	}
}
