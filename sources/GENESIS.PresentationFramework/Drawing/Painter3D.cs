using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using Hexa.NET.ImGui;

namespace GENESIS.PresentationFramework.Drawing {
	
	public abstract class Painter3D : IPainter {

		protected List<DrawList> DrawLists { get; } = [];
		protected int CurrentDrawList { get; set; } = -1;
		protected string? CurrentModel { get; set; } = null;

		public Painter3D() {
		#if DEBUG
			var id = $"Painter3D#{GetHashCode()}";
			
			DebugScene.DebugInfoSlots[id] = delta => {
				if(ImGui.TreeNode("Graphs")) {
					ImGuiExtras.UpdateHistogram($"{id}_delta", (float) delta * 1000.0f);
					ImGuiExtras.DrawHistogram(
						$"{id}_delta",
						"DT",
						$"{Math.Round(1000.0 / delta / 1000.0)} FPS",
						new Vector2(300, 40)
					);
				
					ImGui.TreePop();
				}
				
				ImGui.SeparatorText($"Draw Lists ({DrawLists.Count})");

				for(int i = 0; i < DrawLists.Count; i++) {
					var drawList = DrawLists[i];

					if(ImGui.TreeNode(i.ToString())) {
						ImGui.Text(drawList.Enabled ? "Enabled" : "Disabled");
						ImGui.SameLine();
						
						if(ImGui.Button("Toggle")) {
							drawList.Enabled = !drawList.Enabled;
						}
						
						if(ImGui.TreeNode($"Vertices {drawList.Vertices.Count}")) {
							for(int j = 0; j < drawList.Vertices.Count; j++) {
								ImGui.Text($"{j}: {drawList.Vertices[j]}");
							}
							
							ImGui.TreePop();
						}
						
						if(ImGui.TreeNode($"Colors {drawList.Colors.Count}")) {
							for(int j = 0; j < drawList.Colors.Count; j++) {
								ImGui.Text($"{j}: {drawList.Colors[j]}");
							}
							
							ImGui.TreePop();
						}
						
						if(ImGui.TreeNode($"Matrices {drawList.Matrices.Count}")) {
							for(int j = 0; j < drawList.Matrices.Count; j++) {
								if(ImGui.TreeNode($"{j}")) {
									var matrix = drawList.Matrices[j];
									
									Matrix4x4.Decompose(
										matrix,
										out var scale,
										out var rotation,
										out var translation
									);

									ImGui.Text(matrix.ToString());
									ImGui.Text($"Translation: {translation}");
									ImGui.Text($"Rotation: {rotation}");
									ImGui.Text($"Scale: {scale}");
									ImGui.TreePop();
								}
							}
							
							ImGui.TreePop();
						}
						
						ImGui.TreePop();
					}
				}
			};
		#endif
		}
		
		public abstract void BeginDrawList();
		public abstract void EndDrawList();
		
		public bool SetDrawList(int index) {
			if(index > DrawLists.Count - 1) return false;

			CurrentDrawList = index;
			CurrentModel = DrawLists[index].Model;
			return true;
		}

		public void AddCube(Vector3 position, Vector3 rotation, Vector3 scale, Color color) {
			Debug.Assert(CurrentDrawList != -1, "AddCube() called outside a draw list");
			Debug.Assert(CurrentModel is null or "cube", "Only a single model can be drawn per draw list");

			var drawList = DrawLists[CurrentDrawList];

			if(CurrentModel is null) {
				CurrentModel = "cube";
				drawList.Model = CurrentModel;
				drawList.Vertices.AddRange(Models.Cube);
			}
			
			// if(!drawList.Models.ContainsKey("cube")) {
			// 	drawList.Objects.Add(Models.Cube);
			// 	drawList.Models["cube"] = (uint) drawList.Objects.Count;
			// }
			
			//drawList.ObjectIndices.Add(drawList.Models["cube"]);
			drawList.Colors.Add(color.ToVector4());
			drawList.Matrices.Add(Matrix4x4.CreateScale(scale)
				* Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
				* Matrix4x4.CreateTranslation(position));
		}

		public void AddObject(string modelName, Vector3 position, Vector3 rotation, Vector3 scale, Color color) {
			Debug.Assert(CurrentDrawList != -1, "AddObject() called outside a draw list");
			Debug.Assert(CurrentModel is null || CurrentModel == modelName, "Only a single model can be drawn per draw list");

			var drawList = DrawLists[CurrentDrawList];
			
			Debug.Assert(drawList.Models.ContainsKey(modelName));

			if(CurrentModel is null) {
				CurrentModel = modelName;
				drawList.Model = CurrentModel;
				drawList.Vertices.AddRange(drawList.Models[modelName]);
			}
			
			//drawList.ObjectIndices.Add(drawList.Models[objectName]);
			drawList.Colors.Add(color.ToVector4());
			drawList.Matrices.Add(Matrix4x4.CreateScale(scale)
			                      * Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
			                      * Matrix4x4.CreateTranslation(position));
		}

		public abstract void Paint();

		public bool RemoveDrawList(int index) {
			if(index > DrawLists.Count - 1) return false;
			
			DrawLists[index].Dispose();
			DrawLists.RemoveAt(index);
			return true;
		}

		public bool ClearDrawList(int index) {
			if(index > DrawLists.Count - 1) return false;
			DrawLists[index].Clear();
			return true;
		}

		public int Clear() {
			var count = DrawLists.Count;
			
			foreach(var drawList in DrawLists) {
				drawList.Dispose();
			}
			
			DrawLists.Clear();
			return count;
		}

		public void Dispose() {
			Clear();
			
		#if DEBUG
			DebugScene.DebugInfoSlots.Remove(ToString());
		#endif
		}
	}
}
