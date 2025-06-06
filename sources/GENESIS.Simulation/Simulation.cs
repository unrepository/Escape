namespace GENESIS.Simulation {
	
	public abstract class Simulation<D, S>
		where D : ISimulationData
		where S : ISimulationState<D>, new() {
		
		public S CurrentState { get; protected set; }

		protected Simulation(D data) {
			CurrentState = new S() {
				Tick = 0,
				Data = data
			};
		}

		public Simulation(S state) {
			CurrentState = state;
		}
		
		public abstract S TickSingle();
	}
}
