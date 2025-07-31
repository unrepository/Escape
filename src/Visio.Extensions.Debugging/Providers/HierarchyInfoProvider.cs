using Arch.Core;
using Arch.Core.Extensions;
using Visio.Components;
using ImGui_ = Hexa.NET.ImGui.ImGui;

namespace Visio.Extensions.Debugging.Providers {
	
	public class HierarchyInfoProvider : DebugInfoProvider {

		public override string Title => "World Hierarchy";

		public World World { get; set; }
		
		public HierarchyInfoProvider(World world) {
			World = world;
		}
		
		public override void Render() {
			void ProcessEntity(Entity parent, Entity entity) {
				if(ImGui_.TreeNode(entity.Id.ToString())) {
					if(ImGui_.TreeNode("Components")) {
						var components = entity.GetAllComponents();

						foreach(var component in components) {
							if(component is null) {
								ImGui_.Text("unknown: null");
								continue;
							}
							
							ImGui_.Text(component.GetType().Name + ": " + component);
						}
						
						ImGui_.TreePop();
					}

					if(entity.HasChildren()) {
						if(ImGui_.TreeNode("Children")) {
							foreach (var child in entity.GetChildren()) {
								ProcessEntity(entity, child);
							}
						
							ImGui_.TreePop();
						}
					}
					
					ImGui_.TreePop();
				}
			}

			ProcessEntity(default, World.GetRootEntity());
		}
	}
}
