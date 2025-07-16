using Cinenic.Renderer;
using Flecs.NET.Core;
using EcsWorld = Flecs.NET.Core.World;

namespace Cinenic.World {
	
	public class WorldRenderer : IRenderable {

		public string Id { get; }
		public RenderPipeline Pipeline { get; set; }
		
		public EcsWorld World { get; set; }
		
		public WorldRenderer(string id, EcsWorld world) {
			Id = id;
			World = world;
		}

		public void Render(RenderQueue queue, TimeSpan delta) {
			World
				.Each((ref Components.Renderable renderable) => {
					renderable.Render.Invoke(delta);
				});
		}
	}
}
