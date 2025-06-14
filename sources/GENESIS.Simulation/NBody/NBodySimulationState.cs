namespace GENESIS.Simulation.NBody {
	
	public struct NBodySimulationState() : ISimulationState<NBodySimulationData> {

		public long Tick { get; set; }
		public NBodySimulationData Data { get; set; }
	}
}
