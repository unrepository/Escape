using Silk.NET.Maths;

namespace GENESIS.Simulation.PlateTectonics {
	
	public struct PlateTectonicsData() : ISimulationData {

		public int Seed { get; init; } = 0;
		
		public Dictionary<Vector2D<int>, TectonicPlateTile> Map { get; init; } = [];
		public Vector2D<int> MapSize { get; init; } = Vector2D<int>.Zero;
	}
}
