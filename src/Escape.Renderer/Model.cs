using System.Numerics;
using Escape.Extensions.CSharp;

namespace Escape.Renderer {
	
	public class Model : ITypeCloneable<Model> {

		public required List<Mesh> Meshes { get; init; }

		public Model Clone() {
			var clonedMeshes = new List<Mesh>(Meshes.Count);

			foreach(var mesh in Meshes) {
				clonedMeshes.Add(mesh.Clone());
			}

			return new() {
				Meshes = clonedMeshes
			};
		}
	}
}
