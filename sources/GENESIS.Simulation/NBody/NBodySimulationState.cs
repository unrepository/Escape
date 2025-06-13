namespace GENESIS.Simulation.NBody {
	
	public struct NBodySimulationState() : ISimulationState<NBodySimulationData> {

		public long Tick { get; init; }
		public NBodySimulationData Data { get; init; }
	}
}
