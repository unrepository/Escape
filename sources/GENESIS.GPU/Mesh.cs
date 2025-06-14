namespace GENESIS.GPU {
	
	public class Mesh {

		public Vertex[] Vertices { get; init; } = [];
		public uint[] Indices { get; init; } = [];

		public Material Material { get; set; }
		public Texture[] Textures { get; init; } = [];

		public static bool operator ==(Mesh a, Mesh b) => a.Equals(b);
		public static bool operator !=(Mesh a, Mesh b) => !a.Equals(b);
		
		public bool Equals(Mesh other) {
			return Vertices.GetHashCode() == other.Vertices.GetHashCode()
				&& Indices.GetHashCode() == other.Indices.GetHashCode();
		}

		public override bool Equals(object? obj) {
			return obj is Mesh other && Equals(other);
		}
	}
}
