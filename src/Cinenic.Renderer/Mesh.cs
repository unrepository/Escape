using Cinenic.Extensions.CSharp;

namespace Cinenic.Renderer {
	
	public class Mesh : ITypeCloneable<Mesh> {

		public Vertex[] Vertices = [];
		public uint[] Indices = [];

		public Material Material = new Material();
		
		[Obsolete]
		public Texture[] Textures = [];

		public Mesh Clone() {
			return new() {
				Vertices = (Vertex[]) Vertices.Clone(),
				Indices = (uint[]) Indices.Clone(),
				Material = Material.Clone()
			};
		}

		/*public static bool operator ==(Mesh a, Mesh b) => a.Equals(b);
		public static bool operator !=(Mesh a, Mesh b) => !a.Equals(b);
		
		public bool Equals(Mesh other) {
			return Vertices.GetHashCode() == other.Vertices.GetHashCode()
				&& Indices.GetHashCode() == other.Indices.GetHashCode();
		}

		public override bool Equals(object? obj) {
			return obj is Mesh other && Equals(other);
		}*/
	}
}
