namespace GENESIS.Simulation.PlateTectonics {
	
	public struct PlateTectonicsState : ISimulationState<PlateTectonicsData> {

		public long Tick { get; init; }
		public PlateTectonicsData Data { get; init; }
	}
}
