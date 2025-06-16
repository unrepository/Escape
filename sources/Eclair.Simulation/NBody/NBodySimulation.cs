using Eclair.UnitTypes;
using NLog;
using Silk.NET.Maths;

namespace Eclair.Simulation.NBody {
	
	public class NBodySimulation : Simulation<NBodySimulationData, NBodySimulationState> {
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public NBodySimulation(NBodySimulationData data) : base(data) {
			foreach(var body in data.Bodies) {
				if(body.Parent is not null) {
					body.Position += body.Parent.Position;
					body.Velocity += body.Parent.Velocity;
				}
			}
		}
		
		public NBodySimulation(NBodySimulationState state) : base(state) { }
		
		public override NBodySimulationState TickSingle(double delta) {
			CurrentState.Tick++;
			
			var forces = _CalculateForces();

			for(int i = 0; i < CurrentState.Data.Bodies.Count; i++) {
				var body = CurrentState.Data.Bodies[i];
				
				var F = forces[i];
				var a = F / body.Mass.Kilograms;

				body.LastPosition = body.Position;
				body.LastVelocity = body.Velocity;

				body.Velocity += a * CurrentState.Data.TimeStep;
				body.Position += body.Velocity * CurrentState.Data.TimeStep;

				/*if(body.Parent is not null) {
					var distance = (body.Position - body.Parent.Position).Length;

					if(body.OrbitApogee is null || distance > body.OrbitApogee.Meters) {
						body.OrbitApogee = Length.FromMeters(distance);
					}

					if(body.OrbitPerigee is null || distance < body.OrbitPerigee.Meters) {
						body.OrbitPerigee = Length.FromMeters(distance);
					}
					
					if(Math.Abs(distance - body.OrbitApogee.Meters) > 1e-3) {
						body.OrbitApoapsis = body.Position;
					}

					if(Math.Abs(distance - body.OrbitPerigee.Meters) > 1e-3) {
						body.OrbitPeriapsis = body.Position;
					}
					
					// TODO tilt
				}*/
				
				_logger.Trace("{Tick}: {Body}", CurrentState.Tick, body);
			}

			return CurrentState;
		}

		private Vector3D<double>[] _CalculateForces() {
			var forces = new Vector3D<double>[CurrentState.Data.Bodies.Count];
			Array.Fill(forces, Vector3D<double>.Zero);

			for(int i = 0; i < CurrentState.Data.Bodies.Count; i++) {
				var a = CurrentState.Data.Bodies[i];
				
				for(int j = 0; j < CurrentState.Data.Bodies.Count; j++) {
					if(i == j) continue;
					var b = CurrentState.Data.Bodies[j];

					var deltaPosition = b.Position - a.Position;
					var distance = deltaPosition.Length;

					if(distance < (a.Radius + b.Radius).Meters) {
						_logger.Warn("Celestial bodies {BodyA} and {BodyB} have collided!", a, b); // TODO
						continue;
					}

					var F = CurrentState.Data.GravitationalConstant 
						* ((a.Mass.Kilograms * b.Mass.Kilograms) / Math.Pow(distance, 2));
					forces[i] += F * deltaPosition / distance;
				}
			}
			
			return forces;
		}
	}
}
