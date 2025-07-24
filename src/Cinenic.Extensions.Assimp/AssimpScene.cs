using Arch.Core;
using Arch.Core.Extensions;
using Cinenic.Components;
using Cinenic.Extensions.Scene;
using Cinenic.Renderer;

namespace Cinenic.Extensions.Assimp {
	
	public class AssimpScene : IScene {

		internal List<Node> Nodes = [];
		
		public World AsWorld() {
			throw new NotImplementedException();
		}
		
		public Entity Export(ref World world, Entity? parent) {
			var rootEntity = parent ?? world.Create();
			
			void ExportNode(ref World world, ref Entity parent, Node node) {
				var entity = world.Create(node.Transform);
				if(node.Model is not null) entity.Add(new RenderableObject(node.Model));

				entity.MakeChildOf(parent);
				
				foreach(var child in node.Children) {
					ExportNode(ref world, ref entity, child);
				}
			}
			
			foreach(var node in Nodes) {
				ExportNode(ref world, ref rootEntity, node);
			}

			return rootEntity;
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			Nodes.Clear();
		}

		internal class Node {

			public List<Node> Children = [];

			public Model? Model;
			public Transform3D Transform;
		}
	}
}
