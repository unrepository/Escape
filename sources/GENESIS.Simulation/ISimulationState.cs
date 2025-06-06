namespace GENESIS.Simulation {
	
	public interface ISimulationState<D>
		where D : ISimulationData {
		
		public long Tick { get; init; }
		public D Data { get; init; }
	}
}
