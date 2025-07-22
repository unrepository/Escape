using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Cinenic.Components;
using Cinenic.Renderer;
using Cinenic.Renderer.Camera;
using NLog;
using Camera3D = Cinenic.Components.Camera3D;
using EcsWorld = Arch.Core.World;

namespace Cinenic {
	
	public class WorldRenderer : IRenderer {

		public string Id { get; }
		public int Priority { get; init; }
		
		public EcsWorld World { get; set; }
		public ObjectRenderer ObjectRenderer { get; }

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private readonly Dictionary<Entity, RenderableObject> _entityRenderableMap = [];
		private readonly QueryDescription _camera3DQuery = new QueryDescription().WithAll<Camera3D>();
		private readonly RenderUpdateSystem _mainSystem;
		
		public WorldRenderer(string id, EcsWorld world, ObjectRenderer objectRenderer) {
			Id = id;
			World = world;
			ObjectRenderer = objectRenderer;
			
			_mainSystem = new RenderUpdateSystem(world, objectRenderer);
			
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
				View = Matrix4x4.Identity
			};
			
			World.Query(in _camera3DQuery, (ref Camera3D c3d) => {
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
			
			_mainSystem.Update(delta);
		}
	}
	
	public partial class RenderUpdateSystem : BaseSystem<EcsWorld, TimeSpan> {

		private ObjectRenderer _objectRenderer;

		public RenderUpdateSystem(EcsWorld world, ObjectRenderer objectRenderer) : base(world) {
			_objectRenderer = objectRenderer;
		}
		
		[Query(Parallel = true)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Camera3D_Update(ref Camera3D c3d) {
			if(!c3d.Enabled) return;
			c3d.Camera.Update();
		}

		[Query(Parallel = true)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Transform3D_Camera3D_Synchronize(ref Camera3D c3d, ref Transform3D t3d) {
			c3d.Camera.Position = t3d.Position;
			c3d.Camera.Target = t3d.Position + Vector3.Transform(Vector3.UnitZ, t3d.Rotation);
		}

		[Query]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Transform3D_Update(ref RenderableObject obj, ref Transform3D t3d) {
			var matrix =
				Matrix4x4.CreateScale(t3d.Scale)
				* Matrix4x4.CreateFromQuaternion(t3d.Rotation)
				* Matrix4x4.CreateTranslation(t3d.Position);

			_objectRenderer.SetMatrix(obj, matrix);
		}
	}
}
