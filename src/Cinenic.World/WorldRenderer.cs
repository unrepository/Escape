using System.Numerics;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer;
using Cinenic.Renderer.Camera;
using Cinenic.World.Components;
using Flecs.NET.Core;
using NLog;
using EcsWorld = Flecs.NET.Core.World;

namespace Cinenic.World {
	
	public class WorldRenderer : IRenderer {

		public string Id { get; }
		public int Priority { get; init; }
		
		public ObjectRenderer ObjectRenderer { get; }
		public EcsWorld World { get; set; }

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly Dictionary<Entity, RenderableObject> _entityRenderableMap = [];
		
		public WorldRenderer(string id, ObjectRenderer objectRenderer, EcsWorld world) {
			Id = id;
			ObjectRenderer = objectRenderer;
			World = world;

			world
				.System<Components.Camera3D, Transform3D>("WorldRenderer - Transform3D->Camera3D sync")
				.Each((ref Components.Camera3D c3d, ref Transform3D t3d) => {
					c3d.Camera.Position = t3d.Position;
					c3d.Camera.Target = t3d.Position + Vector3.Transform(Vector3.UnitZ, t3d.Rotation);
				});
			
			world
				.System<Components.Camera3D>("WorldRenderer - Camera3D update")
				.Each((ref Components.Camera3D c3d) => {
					if(!c3d.Enabled) return;
					c3d.Camera.Update();
				});

			world
				.System<Transform3D, RenderableObject>("WorldRenderer->ObjectRenderer - Transform3D matrix update")
				.Each(((ref Transform3D t3d, ref RenderableObject obj) => {
					var matrix =
						Matrix4x4.CreateScale(t3d.Scale)
						* Matrix4x4.CreateFromQuaternion(t3d.Rotation)
						* Matrix4x4.CreateTranslation(t3d.Position);

					ObjectRenderer.SetMatrix(obj, matrix);
				}));
			
			world
				.Observer<RenderableObject>("WorldRenderer->ObjectRenderer - RenderableObject/Set")
				.Event(Ecs.OnSet)
				.Each((Entity e, ref RenderableObject obj) => {
					_logger.Trace("(ecs observer) RenderableObject/Set");
				
					if(_entityRenderableMap.TryGetValue(e, out var prevRenderable)) {
						_logger.Trace("(ecs observer) RenderableObject/Set - RemoveObject");
						ObjectRenderer.RemoveObject(prevRenderable);
					}
				
					_logger.Trace("(ecs observer) RenderableObject/Set - AddObject");
				
					ObjectRenderer.AddObject(obj);
					_entityRenderableMap[e] = obj;
				});

			world
				.Observer<RenderableObject>("WorldRenderer->ObjectRenderer - RenderableObject/Remove")
				.Event(Ecs.OnRemove)
				.Each((Entity e, ref RenderableObject obj) => {
					_logger.Trace("(ecs observer) RenderableObject/Remove");
					
					_entityRenderableMap.Remove(e);
					ObjectRenderer.RemoveObject(obj);
				});
		}

		public void Render(RenderQueue queue, TimeSpan delta) {
			var cameraData = new CameraData {
				Position = Vector3.One,
				Projection = Matrix4x4.Identity,
				View = Matrix4x4.Identity
			};
			
			World
				.Each((ref Components.Camera3D c3d) => {
					if(!c3d.Enabled) return;
					
					cameraData.Position = c3d.Camera.Position;
					// i don't know what this would do, but might produce some funny effects
					// of course, there should also be a way to set a specific camera as the "current" one
					//   and disable all other cameras TODO
					cameraData.Projection *= c3d.Camera.ProjectionMatrix;
					cameraData.View *= c3d.Camera.ViewMatrix;
				});

			ObjectRenderer.ShaderPipeline.CameraData.Data = cameraData;

			unsafe {
				ObjectRenderer.ShaderPipeline.CameraData.Size = (uint) sizeof(CameraData);
			}
		}
	}
}
