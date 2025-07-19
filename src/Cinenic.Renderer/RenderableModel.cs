using System.Numerics;
using Cinenic.Extensions.CSharp;

namespace Cinenic.Renderer {
	
	public class RenderableModel : ITypeCloneable<RenderableModel> {

		public List<Mesh> Meshes { get; init; }

		public RenderableModel Clone() {
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
