using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Cinenic.Components;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer;
using Cinenic.Renderer.Camera;
using Cinenic.Renderer.Lights;
using Cinenic.Systems;
using NLog;
using Camera3D = Cinenic.Components.Camera3D;
using DirectionalLight = Cinenic.Renderer.Lights.DirectionalLight;
using PointLight = Cinenic.Renderer.Lights.PointLight;
using SpotLight = Cinenic.Renderer.Lights.SpotLight;

namespace Cinenic {
	
	public class WorldRenderer : IRenderer {

		public string Id { get; }
		public int Priority { get; init; }
		
		public World World { get; set; }
		public ObjectRenderer ObjectRenderer { get; }

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
		private readonly Dictionary<Entity, RenderableObject> _entityRenderableMap = [];
		
		private readonly QueryDescription _camera3DQuery = new QueryDescription().WithAll<Camera3D>();
		private readonly QueryDescription _directionalLightQuery = new QueryDescription().WithAll<Transform3D, Components.DirectionalLight>();
		private readonly QueryDescription _pointLightQuery = new QueryDescription().WithAll<Transform3D, Components.PointLight>();
		private readonly QueryDescription _spotLightQuery = new QueryDescription().WithAll<Transform3D, Components.SpotLight>();
		
		private readonly List<Renderer.Lights.DirectionalLight> _directionalLights = [];
		private readonly List<Renderer.Lights.PointLight> _pointLights = [];
		private readonly List<Renderer.Lights.SpotLight> _spotLights = [];
		
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
			var shaderPipeline = ObjectRenderer.ShaderPipeline;
			
			// camera update
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
			
			// lighting update (TODO is this the best way to do this?)
			// the lights are in a list outside of this method, as (hopefully) clearing instead of
			// making new instances will lead to less memory pressure
			_directionalLights.Clear();
			World.Query(in _directionalLightQuery, (ref Transform3D t3d, ref Components.DirectionalLight light) => {
				_directionalLights.Add(new() {
					Color = light.Color * light.Intensity,
					Direction = t3d.GlobalRotation.GetDirectionVector()
				});
			});

			_pointLights.Clear();
			World.Query(in _pointLightQuery, (ref Transform3D t3d, ref Components.PointLight light) => {
				_pointLights.Add(new() {
					Color = light.Color * light.Intensity,
					Position = t3d.GlobalPosition
				});
			});

			_spotLights.Clear();
			World.Query(in _spotLightQuery, (ref Transform3D t3d, ref Components.SpotLight light) => {
				_spotLights.Add(new() {
					Color = light.Color * light.Intensity,
					Position = t3d.GlobalPosition,
					Direction = t3d.GlobalRotation.GetDirectionVector(),
					Cutoff = light.Cutoff.Radians,
					CutoffOuter = light.CutoffOuter.Radians
				});
			});
			
			// set data (uploaded in object renderer)
			unsafe {
				shaderPipeline.CameraData.Data = cameraData;
				
				shaderPipeline.LightData.Data = new LightData {
					DirectionalCount = (uint) _directionalLights.Count,
					PointCount = (uint) _pointLights.Count,
					SpotCount = (uint) _spotLights.Count
				};
				
				shaderPipeline.DirectionalLightData.Size = (uint) (_directionalLights.Count * sizeof(DirectionalLight));
				shaderPipeline.PointLightData.Size = (uint) (_pointLights.Count * sizeof(PointLight));
				shaderPipeline.SpotLightData.Size = (uint) (_spotLights.Count * sizeof(SpotLight));
				
				shaderPipeline.DirectionalLightData.Data = _directionalLights.ToArrayNoCopy();
				shaderPipeline.PointLightData.Data = _pointLights.ToArrayNoCopy();
				shaderPipeline.SpotLightData.Data = _spotLights.ToArrayNoCopy();
			}
			
			_primarySystem.Update(delta);
		}
	}
}
