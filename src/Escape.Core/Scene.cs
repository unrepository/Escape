using System.Text.Json.Serialization;
using Arch.Core;
using Escape.Renderer;

namespace Escape.Core {
	
	public class Scene : IDisposable {
		
		public IPlatform Platform { get; }
		
		public string Id { get; }
		public World World { get; }

		public Entity Root => World.GetRootEntity();

		public RenderQueue? RenderQueue {
			get => field;
			set {
				if(value is not null) {
					if(WorldRenderer is not null) {
						WorldRenderer.ObjectRenderer = ObjectRenderer.Create(Platform, value.Pipeline!.ShaderPipeline);
					} else {
						WorldRenderer = new WorldRenderer(Id + "#world", World, ObjectRenderer.Create(Platform, value.Pipeline!.ShaderPipeline));
					}
				}

				field = value;
			}
		}
		
		public WorldUpdater WorldUpdater { get; private set; }
		public WorldRenderer? WorldRenderer { get; private set; }

		public Scene(IPlatform platform, string id, World? world, RenderQueue? renderQueue) {
			Platform = platform;
			Id = id;
			World = world ?? World.Create();
			
			WorldUpdater = new WorldUpdater(Platform, Id + "#world", World);
			
			// if(RenderQueue is not null) {
			// 	WorldRenderer = new WorldRenderer(Id + "#world", World, ObjectRenderer.Create(Platform, RenderQueue.Pipeline!.ShaderPipeline));
			// }
			
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
				WorldRenderer.Dispose();
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
