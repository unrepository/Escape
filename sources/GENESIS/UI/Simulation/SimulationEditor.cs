using System.Numerics;
using Eclair.Renderer;
using Eclair.Presentation;
using GENESIS.Project;
using Eclair.Simulation;
using Hexa.NET.ImGui;

namespace GENESIS.UI.Simulation {
	
	public abstract class SimulationEditor<TSimulationData, TSimulationState> : ImGuiScene
		where TSimulationData : ISimulationData
		where TSimulationState : ISimulationState<TSimulationData>, new()
	{
		protected Simulation<TSimulationData, TSimulationState>? Simulation { get; set; }
		protected IProjectObject Object { get; }
		
		public SimulationEditor(IPlatform platform, string id,
		                        IProjectObject obj) : base(platform, id) {
			Object = obj;
		}
		
		protected override void Paint(double delta) {
			if(ImGui.Begin($"{GetType().Name} - {Object.File.Name}", ref IsOpen,
				   ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.HorizontalScrollbar))
			{
				if(ImGui.BeginMenuBar()) {
					if(ImGui.BeginMenu("File")) {
						if(ImGui.MenuItem("Save")) {
						
						}

						ImGui.EndMenu();
					}

					ImGui.EndMenuBar();
				}
				
				if(Simulation is null) {
					PaintInitializer(delta);
				} else {
					PaintEditor(delta);
				}
			}
				
			ImGui.End();
		}

		public abstract void PaintInitializer(double delta);
		public abstract void PaintEditor(double delta);
	}
}
