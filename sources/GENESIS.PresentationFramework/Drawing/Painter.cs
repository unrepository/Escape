using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using GENESIS.GPU;
using GENESIS.LanguageExtensions;
using Hexa.NET.ImGui;
using ILGPU.Util;
using NLog;

namespace GENESIS.PresentationFramework.Drawing {
	
	public abstract partial class Painter : IDisposable {
		
		public IPlatform Platform { get; }
		public int CurrentDrawList { get; protected set; } = -1;
		
		protected List<DrawList> DrawLists { get; } = [];

		public Painter(IPlatform platform) {
			Platform = platform;
			
		#if DEBUG
			var id = $"Painter#{GetHashCode()}";
			
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
						ImGui.Text(drawList.IsEnabled ? "Enabled" : "Disabled");
						ImGui.SameLine();
						
						if(ImGui.Button("Toggle")) {
							drawList.IsEnabled = !drawList.IsEnabled;
						}
						
						/*if(ImGui.TreeNode($"Vertices {drawList.Vertices.Count}")) {
							for(int j = 0; j < drawList.Vertices.Count; j++) {
								ImGui.Text($"{j}: {drawList.Vertices[j]}");
							}
							
							ImGui.TreePop();
						}*/
						
						if(ImGui.TreeNode($"Materials {drawList.Materials.Count}")) {
							for(int j = 0; j < drawList.Materials.Count; j++) {
								ImGui.Text($"{j}: {drawList.Materials[j]}");
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
		
		public abstract int BeginDrawList(DrawList.ShapeType type = DrawList.ShapeType.Triangle, bool instanced = false);
		public abstract void EndDrawList();
		
		public bool SetDrawList(int index) {
			if(index > DrawLists.Count - 1) return false;

			CurrentDrawList = index;
			return true;
		}

		public void SetModel(Model model) {
			Debug.Assert(CurrentDrawList != -1, "SetModel() called outside a draw list");
			var drawList = DrawLists[CurrentDrawList];
			Debug.Assert(drawList.IsInstanced, "SetModel() can only be called inside an instanced draw list");

			drawList.Meshes.Clear();
			drawList.Meshes.AddRange(model.Meshes);
		}

		public void SetMaterial(Material material, int index = -1) {
			Debug.Assert(CurrentDrawList != -1, "SetMaterial() called outside a draw list");

			var drawList = DrawLists[CurrentDrawList];
			index = index < 0 ? drawList.Materials.Count - 1 : index;
			drawList.Materials[index] = material;
		}

		public void SetTextures(
			Texture diffuse,
			Texture? normal = null,
			Texture? roughness = null,
			Texture? metallic = null,
			int index = -1
		) {
			Debug.Assert(CurrentDrawList != -1, "SetTextures() called outside a draw list");
			var drawList = DrawLists[CurrentDrawList];
			//Debug.Assert(drawList.IsInstanced, "SetTextures() can only be called in an instanced draw list");

			if(drawList.IsInstanced) {
				drawList.Textures.Clear();
				drawList.Textures.Add([diffuse, normal, roughness, metallic]);
			} else {
				index = index < 0 ? drawList.Materials.Count - 1 : index;
				drawList.Textures[index] = [diffuse, normal, roughness, metallic];

				var use = Material.TextureType.Diffuse;
				if(normal is not null) use |= Material.TextureType.Normal;
				if(roughness is not null) use |= Material.TextureType.Roughness;
				if(metallic is not null) use |= Material.TextureType.Metallic;
				
				CollectionsMarshal.AsSpan(drawList.Materials)[index].UseTextures = use;
			}
		}
		
		public void UseTextures(Material.TextureType use, int index = -1) {
			Debug.Assert(CurrentDrawList != -1, "UseTextures() called outside a draw list");
			var drawList = DrawLists[CurrentDrawList];
			//Debug.Assert(drawList.IsInstanced, "UseTextures() can only be called in an instanced draw list");
			
			index = index < 0 ? drawList.Materials.Count - 1 : index;
			CollectionsMarshal.AsSpan(drawList.Materials)[index].UseTextures = use;
		}
		
		public void SetTransform(int index, Vector3? position = null, Vector3? rotation = null, Vector3? scale = null) {
			Debug.Assert(CurrentDrawList != -1, "SetTransform() called outside a draw list");

			var matrix = DrawLists[CurrentDrawList].Matrices[index];

			var oScale = Vector3.One;
			var oRotation = Quaternion.Zero;
			var oPosition = Vector3.Zero;

			if(!position.HasValue || !rotation.HasValue || !scale.HasValue) {
				Matrix4x4.Decompose(matrix, out oScale, out oRotation, out oPosition);
			}

			var newMatrix = Matrix4x4.CreateScale(scale ?? oScale);

			if(rotation.HasValue) {
				newMatrix *= Matrix4x4.CreateFromYawPitchRoll(rotation.Value.Y, rotation.Value.X, rotation.Value.Z);
			} else {
				newMatrix *= Matrix4x4.CreateFromQuaternion(oRotation);
			}
			
			newMatrix *= Matrix4x4.CreateTranslation(position ?? oPosition);
			DrawLists[CurrentDrawList].Matrices[index] = newMatrix;
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
			DebugScene.DebugInfoSlots.Remove($"Painter#{GetHashCode()}");
		#endif
		}
	}
}
