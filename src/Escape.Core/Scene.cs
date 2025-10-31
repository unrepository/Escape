using System.Text.Json.Serialization;
using Arch.Core;
using Escape.Renderer;

namespace Escape.Core {
	
	public class Scene : IDisposable {
		
		[JsonIgnore] public IPlatform Platform { get; }
		
		public string Id { get; }
		public World World { get; }

		[JsonIgnore] public Entity Root => World.GetRootEntity();
		
		[JsonIgnore] public RenderQueue? RenderQueue { get; }
		
		[JsonIgnore] protected WorldUpdater WorldUpdater { get; }
		[JsonIgnore] protected WorldRenderer? WorldRenderer { get; }

		public Scene(IPlatform platform, string id, World? world, RenderQueue? renderQueue) {
			Platform = platform;
			Id = id;
			World = world ?? World.Create();

			WorldUpdater = new WorldUpdater(platform, id + "#world", World);

			if(renderQueue is not null) {
				WorldRenderer = new WorldRenderer(id + "#world", World, ObjectRenderer.Create(renderQueue.Platform, renderQueue.Pipeline!.ShaderPipeline));
			}
			
			RenderQueue = renderQueue;

			ESCAPE.OnCleanup += () => {
				OnClose();
				Dispose();
			};
		}

		public virtual void OnOpen() { }
		public virtual void OnClose() { }

		public void Open() {
			UpdateManager.Add(WorldUpdater);
			WorldUpdater.RelationshipTracker.Active = true;

			if(RenderQueue is not null && WorldRenderer is not null) {
				RenderManager.Add(RenderQueue, WorldRenderer);
			}
			
			OnOpen();
		}
		
		public void Close() {
			UpdateManager.Remove(WorldUpdater.Id);
			
			if(RenderQueue is not null && WorldRenderer is not null) {
				RenderManager.Remove(RenderQueue, WorldRenderer);
			}
			
			OnClose();
		}

		/*public Entity Instantiate(Scene scene, Entity? parent = null, bool doEvents = true) {
			if(doEvents) scene.OnOpen();
			return World.Instantiate(scene.World, parent);
		}*/

		public virtual void Dispose() {
			GC.SuppressFinalize(this);
			World.Dispose();
		}
	}
}
