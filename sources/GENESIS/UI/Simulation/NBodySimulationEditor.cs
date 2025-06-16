using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using Eclair.Renderer;
using GENESIS.LanguageExtensions;
using Eclair.Presentation.Dialog;
using Eclair.Presentation.Extensions;
using GENESIS.Project;
using GENESIS.Simulation.NBody;
using GENESIS.UnitTypes;
using Hexa.NET.ImGui;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace GENESIS.UI.Simulation {
	
	public class NBodySimulationEditor : SimulationEditor<NBodySimulationData, NBodySimulationState> {
		
		private string _newBodyName = "";
		private readonly NBodySimulationData _initialSimulationData = new() {
			Bodies = []
		};
		
		private NBodySimulationViewer _viewer;
		private NBodySimulation? _futureSimulation;
		
		private readonly List<CelestialBody> _customBodyDatabase = [];
		private TextPrompt? _newCustomBodyPrompt;
		private ListPrompt? _bodySpawnParentPrompt;
		
		public NBodySimulationEditor(IPlatform platform, IProjectObject obj)
			: base(platform, "sim_nb_editor", obj)
		{
			if(obj.File.Exists) {
				Console.WriteLine("NBodySimulationEditor: ADD LOADING FROM FILE!!!");
			}
		}

		public override void Update(double delta) {
			base.Update(delta);

			if(Simulation is null) return;
			if(ImIO.WantCaptureMouse || _bodySpawnParentPrompt?.IsOpen == true) return;
			if(_viewer.SpawningBody is null) return;

			if(Window!.Input!.Mice[0].IsButtonPressed(MouseButton.Left)) {
				// step 1 - position
				//if(_viewer.SpawnState == 0) {
					_viewer.SpawningBody.Position = new Vector3D<double>(
						_viewer.SpawningBodyPosition.X / NBodySimulationViewer.RENDER_SCALE,
						_viewer.SpawningBodyPosition.Y / NBodySimulationViewer.RENDER_SCALE,
						_viewer.SpawningBodyPosition.Z / NBodySimulationViewer.RENDER_SCALE
					);
				
					_viewer.SpawningBody.RData = new CelestialBodyRenderData {
						Color = Color.White.Randomize()
					};
				
					Simulation.CurrentState.Data.Bodies.Add(_viewer.SpawningBody);
				
					_viewer.SpawningBody = null;
					//_viewer.DoUpdates = false;

					//_viewer.SpawnState = 1;
				//}
			} /*else if(Window!.Input!.Keyboards[0].IsKeyPressed(Key.V)) {
				if(_viewer.SpawnState == 1) {
					Console.WriteLine("AAAAAA");
					// step 2 - velocity
					_viewer.SpawningBody.Velocity = _viewer.SpawningBody.Position - new Vector3D<double>(
						_viewer.SpawningBodyPosition.X / NBodySimulationViewer.RENDER_SCALE,
						_viewer.SpawningBodyPosition.Y / NBodySimulationViewer.RENDER_SCALE,
						_viewer.SpawningBodyPosition.Z / NBodySimulationViewer.RENDER_SCALE
					);
					
					_viewer.SpawningBody = null;
					//_viewer.DoUpdates = true;
					_viewer.SpawnState = 0;
				}
			}*/
		}

		protected override void Paint(double delta) {
			ImGui.SetNextWindowSizeConstraints(
				new Vector2(200, 200),
				new Vector2(500, 700)
			);
			
			base.Paint(delta);
		}

		public override void PaintInitializer(double delta) {
			var dataErrors = false;
			
			for(int i = 0; i < _initialSimulationData.Bodies.Count; i++) {
				var body = _initialSimulationData.Bodies[i];
				
				ImGui.PushID($"body{i}");
				ImGui.SeparatorText($"Body {i}: {body.Name}");

				dataErrors = _BodyEditor(body, _initialSimulationData.Bodies, true, true, true);
				
				ImGui.PopID();
			}
			
			ImGui.SeparatorText("New body");

			ImGui.InputTextWithHint("##newBodyName", "Name", ref _newBodyName, 64);
			ImGui.SameLine();

			if(ImGui.Button("+")) {
				_initialSimulationData.Bodies.Add(new CelestialBody {
					Name = _newBodyName,
					RData = new CelestialBodyRenderData()
				});

				_newBodyName = "";
			}
			
			ImGui.Separator();

			if(!dataErrors && ImGui.Button("OK")) {
				NBodySimulation simulation;
				
			#if DEBUG
				if(_initialSimulationData.Bodies.Count == 0) {
					List<CelestialBody> bodies = [
						new CelestialBody {
							Name = "Sun",
							Mass = Mass.FromKilograms(1.9885e30),
							Radius = Length.FromKilometers(696340),
							Position = Vector3D<double>.Zero,
							Velocity = Vector3D<double>.Zero,
							RData = new CelestialBodyRenderData {
								Color = Color.Yellow
							}
						},
						new CelestialBody {
							Name = "Earth",
							Mass = Mass.FromKilograms(5.972e24),
							Radius = Length.FromKilometers(6378),
							Position = new(147.1e9, 0, 0),
							Velocity = new(0, 0, 30290),
							RData = new CelestialBodyRenderData {
								Color = Color.Blue
							}
						},
						new CelestialBody {
							Name = "Moon",
							Mass = Mass.FromKilograms(7.3477e22),
							Radius = Length.FromKilometers(1737),
							Position = new(393.3e6, 0, 0),
							Velocity = new(0, 0, 1082),
							RData = new CelestialBodyRenderData {
								Color = Color.Gray
							}
						},
					];

					bodies[1].Parent = bodies[0];
					bodies[2].Parent = bodies[1];

					simulation = new NBodySimulation(new NBodySimulationData {
						Bodies = bodies
					});
				} else {
			#endif
					simulation = new NBodySimulation(_initialSimulationData);
			#if DEBUG
				}
			#endif
				
				Simulation = simulation;
				_viewer = new NBodySimulationViewer(Platform, simulation);
				Window!.ScheduleLater(() => Window.PushScene(_viewer));
			}
		}

		public override void PaintEditor(double delta) {
			if(ImGui.BeginTabBar("editor")) {
				if(ImGui.BeginTabItem("Simulation")) {
					ImGui.Checkbox("Do Updates", ref _viewer.DoUpdates);

					int ups = (int) _viewer.UpdatesPerSecond;
					ImGui.InputInt("Updates per second", ref ups, 1, 1000);
					_viewer.UpdatesPerSecond = ups;

					ImGui.InputInt("Ticks per update", ref _viewer.TicksPerUpdate, 1, 100);

					int ts = (int) Simulation!.CurrentState.Data.TimeStep;
					ImGui.InputInt("Time step (seconds)", ref ts, 1, int.MaxValue);

					if(ts != Simulation.CurrentState.Data.TimeStep) {
						Simulation.CurrentState.Data = Simulation.CurrentState.Data with {
							TimeStep = ts
						};
					}
			
					ImGui.SeparatorText($"Bodies ({Simulation.CurrentState.Data.Bodies.Count})");

					foreach(var body in Simulation.CurrentState.Data.Bodies) {
						if(ImGui.TreeNode(body.Name)) {
							if(body.Parent is not null) {
								ImGui.Text($"Orbiting {body.Parent.Name}");
							}
					
							if(ImGui.Button("Focus")) {
								_viewer.ZoomInto(body);
							}
							
							ImGui.Text($"Perigee: {body.OrbitPerigee?.Kilometers} km");
							ImGui.Text($"Periapsis: {body.OrbitPeriapsis}");
							ImGui.Text($"Apogee: {body.OrbitApogee?.Kilometers} km");
							ImGui.Text($"Apoapsis: {body.OrbitApoapsis}");
							ImGui.Text($"Distance to parent: {body.DistanceToParent / 1000.0} km");
							ImGui.Separator();

							_BodyEditor(body, Simulation.CurrentState.Data.Bodies, false, true, true);
					
							/*ImGui.Text($"Position: {body.Position.X:g7}, {body.Position.Y:g7}, {body.Position.X:g7}");
							ImGui.Text($"Velocity: {body.Velocity.X:g7}, {body.Velocity.Y:g7}, {body.Velocity.X:g7}");
					
							var massStr = body.Mass.Kilograms.ToString();
							if(ImGui.InputText("Mass (kg)", ref massStr, 64, ImGuiInputTextFlags.EnterReturnsTrue)) {
								try {
									body.Mass.Kilograms = double.Parse(massStr);
								} catch(FormatException) { }
							}
					
							var radiusStr = body.Radius.Kilometers.ToString();
							if(ImGui.InputText("Radius (km)", ref radiusStr, 64, ImGuiInputTextFlags.EnterReturnsTrue)) {
								try {
									body.Radius.Kilometers = double.Parse(radiusStr);
								} catch(FormatException) { }
							}*/
					
							ImGui.TreePop();
						}
					}
					
					ImGui.EndTabItem();
				}

				if(ImGui.BeginTabItem("Body database")) {
					foreach(var customBody in _customBodyDatabase) {
						if(ImGui.Button(customBody.Name)) {
							_viewer.SpawningBody = customBody;
							_viewer.DoUpdates = false;

							_bodySpawnParentPrompt = new("New body...", "Parent",
								Simulation.CurrentState.Data.Bodies.Select(b => b.Name).ToArray());
						}

						if(ImGui.BeginPopupContextItem((string) null, ImGuiPopupFlags.MouseButtonRight)) {
							_BodyEditor(customBody, _customBodyDatabase, false, false, false);
							
							ImGui.EndPopup();
						}
					}

					if(ImGui.Button("+")) {
						_newCustomBodyPrompt = new("New body...", "Name");
					}
					
					ImGui.EndTabItem();
				}

				/*if(ImGui.BeginTabItem("Body spawner")) {
					ImGui.EndTabItem();
				}*/
				
				ImGui.EndTabBar();
			}

			if(_newCustomBodyPrompt?.Prompt() == true && !string.IsNullOrEmpty(_newCustomBodyPrompt.Result)) {
				_customBodyDatabase.Add(new CelestialBody {
					Name = _newCustomBodyPrompt.Result
				});
			}

			if(_bodySpawnParentPrompt?.Prompt() == true) {
				_viewer.SpawningBody!.Parent =
					Simulation!.CurrentState.Data.Bodies.Single(b => b.Name == _bodySpawnParentPrompt.Result);
			}
		}

		private bool _BodyEditor(
			CelestialBody body, IList<CelestialBody> bodies,
			bool parentEditor, bool positionEditor, bool velocityEditor)
		{
			if(parentEditor) {
				var bodyNames = new[] { "None" }
					.Concat(bodies
					        .Select(b => b.Name)
					        .Where(n => n != body.Name)
					        .ToArray());
			
				int parentIndex = body.Parent is null
					? 0
					: Array.FindIndex(bodyNames, n => n == body.Parent.Name);

				ImGui.Combo(
					"Parent",
					ref parentIndex,
					bodyNames,
					bodyNames.Length
				);

				if(parentIndex == 0) {
					body.Parent = null;
				} else {
					var parentName = bodyNames[parentIndex];
					var parent = _initialSimulationData.Bodies.Single(b => b.Name == parentName);

					body.Parent = parent;
				}
			}

			var massStr = body.Mass.Kilograms.ToString();
			ImGui.InputText("Mass (kg)", ref massStr, 32);
			var radiusStr = body.Radius.Kilometers.ToString();
			ImGui.InputText("Radius (km)", ref radiusStr, 32);

			var positionXStr = "";
			var positionYStr = "";
			var positionZStr = "";
			
			if(positionEditor) {
				positionXStr = body.Position.X.ToString();
				ImGui.InputText("Position X (m)", ref positionXStr, 32);
				positionYStr = body.Position.Y.ToString();
				ImGui.InputText("Position Y (m)", ref positionYStr, 32);
				positionZStr = body.Position.Z.ToString();
				ImGui.InputText("Position Z (m)", ref positionZStr, 32);
			}

			var velocityXStr = "";
			var velocityYStr = "";
			var velocityZStr = "";

			if(velocityEditor) {
				velocityXStr = body.Velocity.X.ToString();
				ImGui.InputText("Velocity X (m/s)", ref velocityXStr, 32);
				velocityYStr = body.Velocity.Y.ToString();
				ImGui.InputText("Velocity Y (m/s)", ref velocityYStr, 32);
				velocityZStr = body.Velocity.Z.ToString();
				ImGui.InputText("Velocity Z (m/s)", ref velocityZStr, 32);
			}

			try {
				body.Mass.Kilograms = double.Parse(massStr);
				body.Radius.Kilometers = double.Parse(radiusStr);

				if(positionEditor) {
					body.Position = new Vector3D<double>(
						double.Parse(positionXStr),
						double.Parse(positionYStr),
						double.Parse(positionZStr)
					);
				}

				if(velocityEditor) {
					body.Velocity = new Vector3D<double>(
						double.Parse(velocityXStr),
						double.Parse(velocityYStr),
						double.Parse(velocityZStr)
					);
				}
			} catch(FormatException) {
				return true;
			}

			return false;
		}
	}
}
