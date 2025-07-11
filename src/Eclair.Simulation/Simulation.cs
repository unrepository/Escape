namespace Eclair.Simulation {
	
	public abstract class Simulation<D, S>
		where D : ISimulationData
		where S : ISimulationState<D>, new() {
		
		public S CurrentState;

		/// <summary>
		/// Initialises a new simulation from the given data
		/// </summary>
		/// <param name="data"></param>
		protected Simulation(D data) {
			CurrentState = new S {
				Tick = 0,
				Data = data
			};
		}

		/// <summary>
		/// Initialises an existing simulation from the given state
		/// </summary>
		/// <param name="state"></param>
		public Simulation(S state) {
			CurrentState = state;
		}
		
		public abstract S TickSingle(double delta);
	}
}
