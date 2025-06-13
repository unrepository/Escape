using System.Drawing;
using GENESIS.UnitTypes;
using Silk.NET.Maths;

namespace GENESIS.Simulation.NBody {
	
	public class CelestialBody {
		
		public string Name { get; init; }
		
		public CelestialBody? Parent { get; set; }
		
		public Mass Mass = Mass.FromKilograms(0);
		public Length Radius = Length.FromMeters(0);
		
		public Vector3D<double> Position = Vector3D<double>.Zero;
		public Vector3D<double> Velocity = Vector3D<double>.Zero;
		
		public Vector3D<double> LastPosition = Vector3D<double>.Zero;
		public Vector3D<double> LastVelocity = Vector3D<double>.Zero;

		public double? DistanceToParent {
			get {
				if(Parent is null) return null;
				return (Position - Parent.Position).Length;
			}
		}
		
		public Length? OrbitApogee;
		public Vector3D<double>? OrbitApoapsis;
		public Length? OrbitPerigee;
		public Vector3D<double>? OrbitPeriapsis;
		public Rotation? OrbitTilt;

		public dynamic? RData;
		
		public CelestialBody() { }

		public CelestialBody(CelestialBody other) {
			Name = other.Name;
			Mass = Mass.FromKilograms(other.Mass.Kilograms);
			Radius = Length.FromKilometers(other.Radius.Kilometers);
			Position = other.Position;
			Velocity = other.Velocity;
			LastPosition = other.LastPosition;
			LastVelocity = other.LastVelocity;
			OrbitApogee = other.OrbitApogee;
			OrbitApoapsis = other.OrbitApoapsis;
			OrbitPerigee = other.OrbitPerigee;
			OrbitPeriapsis = other.OrbitPeriapsis;
			OrbitTilt = other.OrbitTilt;
		}

		public Vector3D<double> RelativePosition(CelestialBody other) =>
			Position - other.Position;

		public override string ToString()
			=> $"{Name}[m={Mass.Kilograms:g2} kg, r={Radius.Kilometers:g2} km, p={Position:g2}, V={Velocity:g2}]";
	}
}
