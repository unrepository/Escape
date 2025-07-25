using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Cinenic.Components;
using Cinenic.Renderer;
using Cinenic.Renderer.Camera;
using Cinenic.Systems;
using NLog;
using Camera3D = Cinenic.Components.Camera3D;

namespace Cinenic {
	
	public class WorldRenderer : IRenderer {

		public string Id { get; }
		public int Priority { get; init; }
		
		public World World { get; set; }
		public ObjectRenderer ObjectRenderer { get; }

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private readonly Dictionary<Entity, RenderableObject> _entityRenderableMap = [];
		
		private readonly QueryDescription _camera3DQuery = new QueryDescription().WithAll<Camera3D>();
		private readonly RenderUpdateSystem _primarySystem;
		
		public WorldRenderer(string id, World world, ObjectRenderer objectRenderer) {
			Id = id;
			World = world;
			ObjectRenderer = objectRenderer;
			
			_primarySystem = new RenderUpdateSystem(world, objectRenderer);
			
			World.SubscribeComponentAdded((in Entity e, ref RenderableObject obj) => {
				_logger.Trace("(ecs observer) RenderableObject/Add");
				_entityRenderableMap[e] = obj;
				ObjectRenderer.AddObject(obj);
			});
			
			World.SubscribeComponentSet((in Entity e, ref RenderableObject obj) => {
				_logger.Trace("(ecs observer) RenderableObject/Set");
				
				if(_entityRenderableMap.TryGetValue(e, out var prevRenderable)) {
					_logger.Trace("(ecs observer) RenderableObject/Set - RemoveObject");
					ObjectRenderer.RemoveObject(prevRenderable);
				}
				
				_logger.Trace("(ecs observer) RenderableObject/Set - AddObject");
				
				ObjectRenderer.AddObject(obj);
				_entityRenderableMap[e] = obj;
			});
			
			World.SubscribeComponentRemoved((in Entity e, ref RenderableObject obj) => {
				_logger.Trace("(ecs observer) RenderableObject/Remove");
					
				_entityRenderableMap.Remove(e);
				ObjectRenderer.RemoveObject(obj);
			});
		}

		public void Render(RenderQueue queue, TimeSpan delta) {
			var cameraData = new CameraData {
				Position = Vector3.One,
				Projection = Matrix4x4.Identity,
				InverseProjection = Matrix4x4.Identity,
				View = Matrix4x4.Identity,
				InverseView = Matrix4x4.Identity,
				AspectRatio = 0
			};
			
			World.Query(in _camera3DQuery, (ref Camera3D c3d) => {
				if(!c3d.Enabled) return;
				
				cameraData.Position = c3d.Camera.Position;
				// i don't know what this would do, but might produce some funny effects
				// of course, there should also be a way to set a specific camera as the "current" one
				//   and disable all other cameras TODO
				cameraData.Projection *= c3d.Camera.ProjectionMatrix;
				cameraData.InverseProjection *= c3d.Camera.InverseProjectionMatrix;
				cameraData.View *= c3d.Camera.ViewMatrix;
				cameraData.InverseView *= c3d.Camera.InverseViewMatrix;
				cameraData.AspectRatio = c3d.Camera.Width / c3d.Camera.Height;
			});

			ObjectRenderer.ShaderPipeline.CameraData.Data = cameraData;

			unsafe {
				ObjectRenderer.ShaderPipeline.CameraData.Size = (uint) sizeof(CameraData);
			}
			
			_primarySystem.Update(delta);
		}
	}
}
