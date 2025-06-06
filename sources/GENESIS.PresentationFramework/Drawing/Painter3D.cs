using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using GENESIS.LanguageExtensions;
using NLog.LayoutRenderers.Wrappers;

namespace GENESIS.PresentationFramework.Drawing {
	
	public abstract class Painter3D {

		protected List<DrawList> DrawLists { get; } = [];

		protected int CurrentDrawList { get; set; } = -1;

		public abstract void BeginDrawList();
		public abstract void EndDrawList();

		public void AddCube(Vector3 position, Vector3 rotation, Vector3 scale, Color color) {
			Debug.Assert(CurrentDrawList != -1, "AddCube() called outside a draw list");

			var drawList = DrawLists[CurrentDrawList];

			if(!drawList.Models.ContainsKey("cube")) {
				drawList.Objects.Add(Models.Cube);
				drawList.Models["cube"] = (uint) drawList.Objects.Count;
			}
			
			drawList.ObjectIndices.Add(drawList.Models["cube"]);
			drawList.Colors.Add(color.ToVector4());
			drawList.Matrices.Add(Matrix4x4.CreateScale(scale)
				* Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
				* Matrix4x4.CreateTranslation(position));
		}

		public void AddObject(string objectName, Vector3 position, Vector3 rotation, Vector3 scale, Color color) {
			Debug.Assert(CurrentDrawList != -1, "AddObject() called outside a draw list");

			var drawList = DrawLists[CurrentDrawList];

			Debug.Assert(drawList.Models.ContainsKey(objectName));
			
			drawList.ObjectIndices.Add(drawList.Models[objectName]);
			drawList.Colors.Add(color.ToVector4());
			drawList.Matrices.Add(Matrix4x4.CreateScale(scale)
			                      * Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
			                      * Matrix4x4.CreateTranslation(position));
		}

		public void RemoveDrawList(int index) {
			DrawLists[index].Dispose();
			DrawLists.RemoveAt(index);
		}

		public void Clear() {
			foreach(var drawList in DrawLists) {
				drawList.Dispose();
			}
			
			DrawLists.Clear();
		}
	}
}
