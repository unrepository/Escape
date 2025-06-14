using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using GENESIS.GPU;
using GENESIS.LanguageExtensions;
using GENESIS.PresentationFramework;
using GENESIS.PresentationFramework.Camera;
using GENESIS.PresentationFramework.Drawing;
using GENESIS.Simulation;
using GENESIS.Simulation.NBody;
using GENESIS.UnitTypes;

namespace GENESIS.UI.Simulation {
	
	public class NBodySimulationViewer : EnvironmentScene {

		public const float RENDER_SCALE = 1 / 1_000_000f;
		
		public NBodySimulation Simulation { get; }
		public NBodySimulation? FutureSimulation { get; private set; }
		public CelestialBody? TargetBody { get; set; }
		
		public CelestialBody? SpawningBody { get; set; }
		public Vector3 SpawningBodyPosition { get; private set; }
		// public Vector3 LastSpawningBodyPosition { get; private set; }
		// public int SpawnState { get; set; } = 0;
		
		public int TicksPerUpdate = 1;

		private OrbitCamera3D _orbitCamera;

		public NBodySimulationViewer(IPlatform platform, NBodySimulation simulation)
			: base(platform, "sim_nb_viewer")
		{
			Simulation = simulation;
		}

		public void ZoomInto(CelestialBody body, int zoom = 10) {
			TargetBody = body;
			_orbitCamera.Distance = body.ScaledRadius() * zoom;
		}

		public override void Initialize(Window window) {
			base.Initialize(window);

			Camera = new PerspectiveCamera3D(window, PrimaryShader) {
				Position = new Vector3(0, 10, 0),
				FieldOfView = 60
			};
			
			(Camera as Camera3D)?.LookAt(Vector3.Zero);

			_orbitCamera = new OrbitCamera3D((Camera3D) Camera, window) {
				AllowPanning = false
			};
		}

		public override void Update(double delta) {
			base.Update(delta);
			
			for(int i = 0; i < TicksPerUpdate; i++) {
				Simulation.TickSingle();
			}
			
			// data update
			foreach(var body in Simulation.CurrentState.Data.Bodies) {
				if(body.Parent is null) continue;
				var rData = (CelestialBodyRenderData) body.RData!;

			#region Position history update for trail
				if(rData.PositionHistory.Count > rData.TrailLength - 1) {
					rData.PositionHistory.TryDequeue(out _);
				}

				/*if(
                    data.PositionHistory.Count > 1
                    && (((Vector3) body.LastPosition * Scale)
                     - data.PositionHistory[^1])
                    .Length() < 0.001f
                ) continue;*/

				var relativeParentPosition =
					((Vector3) body.LastPosition - (Vector3) body.Parent.LastPosition) * RENDER_SCALE;
                
				rData.PositionHistory.Enqueue(relativeParentPosition);
                
				// TODO
				/*Painter.CustomModels[body.Name] = Models.Curve(
					rData.PositionHistory.ToArray(),
					body.ScaledRadius() / 2
				);*/
			#endregion
			}
		}

		protected override void Paint(double delta) {
			// camera update
			if(TargetBody is null) {
				_orbitCamera.AllowPanning = true;
			} else {
				_orbitCamera.AllowPanning = false;
				_orbitCamera.Target = TargetBody.ScaledPosition();
				
				// TODO draw velocity vector
			}
			
			_orbitCamera.Update(delta);
			
			// raycast update
			if(SpawningBody is not null) {
				//LastSpawningBodyPosition = SpawningBodyPosition;
				
				SpawningBodyPosition = RayCast.ToHorizontalPlane(
					Window!.Input!.Mice[0].Position,
					Window.Base.FramebufferSize,
					_orbitCamera.Camera.Position,
					_orbitCamera.Camera.InverseViewMatrix,
					_orbitCamera.Camera.InverseProjectionMatrix
				);
			}
			
			Painter.Clear();
			
			// body trails
			foreach(var body in Simulation.CurrentState.Data.Bodies) {
				if(body.Parent is null) continue;
				if(body.OrbitApogee is null || body.OrbitPerigee is null) continue;
				
				//TODO if(!Painter.CustomModels.ContainsKey(body.Name)) continue;
				
				Painter.BeginDrawList(DrawList.ShapeType.TriangleStrip);
				/*Painter.Add3DObject(
					body.Name,
					body.Parent.ScaledPosition(),
					Vector3.Zero,
					Vector3.One,
					Color.White
				); TODO*/ 
				Painter.EndDrawList();
			}
			
			Painter.BeginDrawList();
			foreach(var body in Simulation.CurrentState.Data.Bodies) {
				var position = body.ScaledPosition();
				var rotation = Vector3.Zero;
				var scale = body.ScaledRadius();
				
				Painter.Add3DCube(position, rotation, new Vector3(scale), body.RData!.Color);
			}
			Painter.EndDrawList();
			
			// spawning body
			if(SpawningBody is not null) {
				Painter.BeginDrawList();
				Painter.Add3DCube(
					SpawningBodyPosition,
					Vector3.Zero,
					new Vector3(SpawningBody.ScaledRadius()),
					Color.Chocolate
				);
				Painter.EndDrawList();
				
				/*if(SpawnState == 0) {
					Painter.BeginDrawList();
					Painter.Add3DCube(SpawningBodyPosition, Vector3.Zero, Vector3.One, Color.Chocolate);
					Painter.EndDrawList();
				} else if(SpawnState == 1) {
					if(LastSpawningBodyPosition != SpawningBodyPosition) {
						var b = Simulation.CurrentState.Data.Bodies.ConvertAll(b => new CelestialBody(b));

						foreach(var a in b) {
							var x = Simulation.CurrentState.Data.Bodies.Single(d => d.Name == a.Name);
							if(x.Parent is null) continue;
							a.Parent = b.Single(d => d.Name == x.Parent.Name);
						}
						
						FutureSimulation = new NBodySimulation(Simulation.CurrentState with {
							Data = Simulation.CurrentState.Data with {
								//Bodies = new(Simulation.CurrentState.Data.Bodies),
								Bodies = b,
								TimeStep = 60
							}
						});

						var body = FutureSimulation.CurrentState.Data.Bodies
						                           .Single(b => b.Name == SpawningBody.Name);
						List<Vector3> trails = [];

						for(int i = 0; i < 100; i++) {
							FutureSimulation.TickSingle();
							trails.Add((Vector3) body.RelativePosition(body.Parent) * RENDER_SCALE);
						}
						
						// draw trail
						Painter.CustomModels["spawn_trail"] = Models.Curve(trails, 5);
						
						Painter.BeginDrawList(DrawList.ShapeType.TriangleStrip);
						Painter.Add3DObject(
							"spawn_trail",
							body.Parent.ScaledPosition(),
							Vector3.Zero,
							Vector3.One,
							Color.White
						);
						Painter.EndDrawList();
					}
				}*/
			}
			
			base.Paint(delta);
		}
	}
	
	internal class CelestialBodyRenderData {

		public Color Color { get; set; } = Color.White.Randomize();
		public ConcurrentQueue<Vector3> PositionHistory { get; } = [];
		
		public int TrailLength { get; set; } = 100;
	}

	internal static class CelestialBodyRenderExtensions {

		public static Vector3 ScaledPosition(this CelestialBody body)
			=> (Vector3) body.Position * NBodySimulationViewer.RENDER_SCALE;

		public static float ScaledRadius(this CelestialBody body)
			=> (float) body.Radius.Meters * NBodySimulationViewer.RENDER_SCALE;
	}
}
