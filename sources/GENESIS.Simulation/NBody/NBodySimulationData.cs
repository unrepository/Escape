using System.Numerics;

namespace GENESIS.Simulation.NBody {
	
	public struct NBodySimulationData() : ISimulationData {

		public double GravitationalConstant { get; set; } = 6.674301515e-11;
		public long TimeStep { get; set; } = 60 * 60; // one hour
		
		public List<CelestialBody> Bodies { get; init; }
	}
}
