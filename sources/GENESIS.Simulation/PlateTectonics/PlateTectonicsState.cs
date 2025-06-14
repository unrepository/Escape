namespace GENESIS.Simulation.PlateTectonics {
	
	public struct PlateTectonicsState : ISimulationState<PlateTectonicsData> {

		public long Tick { get; set; }
		public PlateTectonicsData Data { get; set; }
	}
}
