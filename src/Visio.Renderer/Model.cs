using System.Numerics;
using Visio.Extensions.CSharp;

namespace Visio.Renderer {
	
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
