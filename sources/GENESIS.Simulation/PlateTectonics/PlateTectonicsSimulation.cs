using System.Numerics;
using Silk.NET.Maths;

namespace GENESIS.Simulation.PlateTectonics {
	
	public class PlateTectonicsSimulation
		: Simulation<PlateTectonicsData, PlateTectonicsState> {

		public PlateTectonicsSimulation(PlateTectonicsData data) : base(data) {
			// flood-fill map
			int remainingTiles = CurrentState.Data.MapSize.X * CurrentState.Data.MapSize.Y - CurrentState.Data.Map.Count;

			//var rnd = new Random(CurrentState.Data.Seed);
			
			while(remainingTiles > 0) {
				/*// pick random tile
				var item = CurrentState.Data.Map.ElementAt(rnd.Next(0, CurrentState.Data.Map.Count));
				var position = item.Key;
				var tile = item.Value;
				
				// pick random direction
				var offsets = new Vector2D<int>[] {
					new Vector2D<int>(position.X - 1, position.Y),
					new Vector2D<int>(position.X + 1, position.Y),
					new Vector2D<int>(position.X, position.Y - 1),
					new Vector2D<int>(position.X, position.Y + 1)
				};

				var offset = offsets[rnd.Next(0, offsets.Length)];
				
				// make sure Y is in bounds, wraparound X if needed
				if(offset.Y < 0 || offset.Y >= CurrentState.Data.MapSize.Y) continue;
		
				if(offset.X < 0 || offset.X >= CurrentState.Data.MapSize.X) {
					offset.X = Math.Max(offset.X, CurrentState.Data.MapSize.X) -
					           Math.Abs(Math.Max(offset.X, CurrentState.Data.MapSize.X));
				}
				
				// set to current plate if not already set
				if(!CurrentState.Data.Map.TryGetValue(offset, out _)) {
					CurrentState.Data.Map[offset] = new TectonicPlateTile() {
						PlateIndex = tile.PlateIndex,
					};
					
					remainingTiles--;
				}*/

				foreach(var (position, tile) in new Dictionary<Vector2D<int>, TectonicPlateTile>(CurrentState.Data.Map)) {
					if(tile.HasGrown) continue;
				
					var offsets = new Vector2D<int>[] {
						new Vector2D<int>(position.X - 1, position.Y),
						new Vector2D<int>(position.X + 1, position.Y),
						new Vector2D<int>(position.X, position.Y - 1),
						new Vector2D<int>(position.X, position.Y + 1)
					};
				
					foreach(var offset in offsets) {
						if(offset.Y < 0 || offset.Y >= CurrentState.Data.MapSize.Y) continue;
						
						var fx = offset;
				
						if(offset.X < 0 || offset.X >= CurrentState.Data.MapSize.X) {
							fx.X = Math.Max(offset.X, CurrentState.Data.MapSize.X) -
							           Math.Abs(Math.Max(offset.X, CurrentState.Data.MapSize.X));
						}
				
						if(!CurrentState.Data.Map.TryGetValue(fx, out _)) {
							CurrentState.Data.Map[fx] = new TectonicPlateTile() {
								PlateIndex = tile.PlateIndex,
							};
							
							remainingTiles--;
						}
					}
				
					tile.HasGrown = true;
				}
			}
			
			// edge detection
			foreach(var (position, tile) in CurrentState.Data.Map) {
				var offsets = new Vector2D<int>[] {
					new Vector2D<int>(position.X - 1, position.Y),
					new Vector2D<int>(position.X + 1, position.Y),
					new Vector2D<int>(position.X, position.Y - 1),
					new Vector2D<int>(position.X, position.Y + 1)
				};
			
				foreach(var offset in offsets) {
					if(offset.X < 0 || offset.X >= CurrentState.Data.MapSize.X ||
					   offset.Y < 0 || offset.Y >= CurrentState.Data.MapSize.Y) {
						continue;
					}
			
					if(CurrentState.Data.Map.TryGetValue(offset, out var oTile)
					   && oTile.PlateIndex != tile.PlateIndex) {
						tile.IsEdge = true;
					}
				}
			}
		}
		
		public PlateTectonicsSimulation(PlateTectonicsState state) : base(state) { }

		public override PlateTectonicsState TickSingle() {
			

			return CurrentState;
		}
	}
}
