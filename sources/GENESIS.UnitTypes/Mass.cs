namespace GENESIS.UnitTypes {
	
	public class Mass {

		public double Tonnes {
			get => Kilograms / 1000.0;
			set => Kilograms = value * 1000.0;
		}
		
		public double Kilograms { get; set; }

		public double Decagrams {
			get => Kilograms * 100.0;
			set => Kilograms = value / 100.0;
		}

		public double Grams {
			get => Kilograms * 1000.0;
			set => Kilograms = value / 1000.0;
		}

		public double SI => Kilograms;

		public static Mass operator +(Mass a, Mass b) => new() { Kilograms = a.Kilograms + b.Kilograms };
		public static Mass operator -(Mass a, Mass b) => new() { Kilograms = a.Kilograms - b.Kilograms };
		
		public static Mass operator *(Mass a, int b) => new() { Kilograms = a.Kilograms * b };
		public static Mass operator *(Mass a, double b) => new() { Kilograms = a.Kilograms * b };
		public static Mass operator /(Mass a, int b) => new() { Kilograms = a.Kilograms / b };
		public static Mass operator /(Mass a, double b) => new() { Kilograms = a.Kilograms / b };

		public static bool operator ==(Mass a, Mass b) => a.Equals(b);
		public static bool operator !=(Mass a, Mass b) => !a.Equals(b);
		
		public bool Equals(Mass a) => Math.Abs(Kilograms - a.Kilograms) < 0.001;
		public override bool Equals(object? obj) => obj is Mass other && Equals(other);

		public override string ToString() => $"{Kilograms} kg";

		public static Mass FromTonnes(double value) => new() { Tonnes = value };
		public static Mass FromKilograms(double value) => new() { Kilograms = value };
		public static Mass FromDecagrams(double value) => new() { Decagrams = value };
		public static Mass FromGrams(double value) => new() { Grams = value };
    }
}
