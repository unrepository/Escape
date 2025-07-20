using System.Numerics;

namespace Cinenic.UnitTypes {
	
	public struct Rotation<T> where T : IFloatingPoint<T> {

		private static readonly T RadToDeg = T.CreateChecked(180 / Math.PI);
		private static readonly T DegToRad = T.CreateChecked(Math.PI / 180);
		
		public T Radians { get; set; }

		public T Degrees {
			get => Radians * RadToDeg;
			set => Radians = DegToRad * value;
		}

		public static Rotation<T> operator +(Rotation<T> a, Rotation<T> b) => new() { Degrees = a.Degrees + b.Degrees };
		public static Rotation<T> operator -(Rotation<T> a, Rotation<T> b) => new() { Degrees = a.Degrees - b.Degrees };
		
		public static Rotation<T> operator *(Rotation<T> a, int b) => new() { Degrees = a.Degrees * T.CreateChecked(b) };
		public static Rotation<T> operator *(Rotation<T> a, T b) => new() { Degrees = a.Degrees * b };
		public static Rotation<T> operator /(Rotation<T> a, int b) => new() { Degrees = a.Degrees / T.CreateChecked(b) };
		public static Rotation<T> operator /(Rotation<T> a, T b) => new() { Degrees = a.Degrees / b };
		
		public static bool operator ==(Rotation<T> a, Rotation<T> b) => a.Equals(b);
		public static bool operator !=(Rotation<T> a, Rotation<T> b) => !a.Equals(b);
		
		public bool Equals(Rotation<T> a) => Math.Abs((dynamic) (Degrees - a.Degrees)) < 0.001;
		public override bool Equals(object? obj) => obj is Rotation<T> other && Equals(other);

		public override string ToString() => $"{Degrees}°";

		public static Rotation<T> FromDegrees(T value) => new() { Degrees = value };
		public static Rotation<T> FromRadians(T value) => new() { Radians = value };
	}
}
