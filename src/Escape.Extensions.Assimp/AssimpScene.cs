using Arch.Core;
using Arch.Core.Extensions;
using Escape.Core;
using Escape.Core.Components;
using Escape.Renderer;

namespace Escape.Extensions.Assimp {
	
	public class AssimpScene : Scene {

		public AssimpScene(string id, List<Node> nodes, RenderQueue? renderQueue) : base(id, null, renderQueue) {
			void ExportNode(World world, Entity parent, Node node) {
				var entity = world.Create(node.Transform);
				if(node.Model is not null) entity.Add(new RenderableObject(node.Model));

				entity.MakeChildOf(parent);
				
				foreach(var child in node.Children) {
					ExportNode(World, entity, child);
				}
			}
			
			foreach(var node in nodes) {
				ExportNode(World, Root, node);
			}
		}

		public class Node {

			public readonly List<Node> Children = [];

			public Model? Model;
			public Transform3D Transform;
		}
	}
}
