namespace Eclair.Simulation {
	
	public interface ISimulationState<D>
		where D : ISimulationData {
		
		public long Tick { get; set; }
		public D Data { get; set; }
	}
}
