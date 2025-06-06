using System.Numerics;

namespace GENESIS.Simulation.PlateTectonics {
	
	public class TectonicPlateTile {
	
		/// <summary>
		/// Position of this tile
		/// </summary>
		//public Vector2 Position { get; init; }

		/// <summary>
		/// Index of the plate that this tile belongs to
		/// </summary>
		public int PlateIndex { get; set; }
		
		public bool IsBase { get; set; }
		public bool IsEdge { get; set; }
		public bool HasGrown { get; set; }

		/// <summary>
		/// Height of this tile
		/// </summary>
		public float Height { get; set; }
	}
}
