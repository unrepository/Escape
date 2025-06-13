using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using GENESIS.LanguageExtensions;

namespace GENESIS.PresentationFramework.Drawing {
	
	public abstract partial class Painter {
		
		public int Add3DCube(Vector3 position, Vector3 rotation, Vector3 scale, Color color) {
			Debug.Assert(CurrentDrawList != -1, "Add3DCube() called outside a draw list");
			Debug.Assert(CurrentModel is null or "cube3d", "Only a single model can be drawn per draw list");

			var drawList = DrawLists[CurrentDrawList];

			if(CurrentModel is null) {
				CurrentModel = "cube3d";
				drawList.Model = CurrentModel;
				drawList.Vertices.AddRange(Models.Cube);
			}
			
			drawList.Colors.Add(color.ToVector4());
			drawList.Matrices.Add(Matrix4x4.CreateScale(scale)
			                      * Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
			                      * Matrix4x4.CreateTranslation(position));

			return drawList.Matrices.Count - 1;
		}
		
		public int Add3DPlane(Vector3 position, Vector3 rotation, Vector3 scale, Color color) {
			Debug.Assert(CurrentDrawList != -1, "Add3DPlane() called outside a draw list");
			Debug.Assert(CurrentModel is null or "plane3d", "Only a single model can be drawn per draw list");

			var drawList = DrawLists[CurrentDrawList];

			if(CurrentModel is null) {
				CurrentModel = "plane3d";
				drawList.Model = CurrentModel;
				drawList.Vertices.AddRange(Models.Quad);
			}
			
			drawList.Colors.Add(color.ToVector4());
			drawList.Matrices.Add(Matrix4x4.CreateScale(scale)
			                      * Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
			                      * Matrix4x4.CreateTranslation(position));
			
			return drawList.Matrices.Count - 1;
		}

		public int Add3DSphere(Vector3 position, Vector3 rotation, float radius, Color color) {
			throw new NotImplementedException();
		}

		// public void Add3DCircleOutlined(Vector3 position, Vector3 rotation, float radius, int segments, Color color) {
		// 	Debug.Assert(CurrentDrawList != -1, "Add3DPlane() called outside a draw list");
		// 	Debug.Assert(CurrentModel is null or "cout3d", "Only a single model can be drawn per draw list");
		//
		// 	var drawList = DrawLists[CurrentDrawList];
		//
		// 	if(CurrentModel is null) {
		// 		CurrentModel = "cout3d";
		// 		drawList.Model = CurrentModel;
		// 		drawList.Vertices.AddRange(Models.Quad);
		// 	}
		// 	
		// 	drawList.Colors.Add(color.ToVector4());
		// 	drawList.Matrices.Add(Matrix4x4.CreateScale(scale)
		// 	                      * Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
		// 	                      * Matrix4x4.CreateTranslation(position));
		// }

		public int Add3DObject(string modelName, Vector3 position, Vector3 rotation, Vector3 scale, Color color) {
			Debug.Assert(CurrentDrawList != -1, "Add3DObject() called outside a draw list");
			Debug.Assert(CurrentModel is null || CurrentModel == modelName, "Only a single model can be drawn per draw list");

			var drawList = DrawLists[CurrentDrawList];
			
			Debug.Assert(CustomModels.ContainsKey(modelName));

			if(CurrentModel is null) {
				CurrentModel = modelName;
				drawList.Model = CurrentModel;
				drawList.Vertices.AddRange(CustomModels[modelName]);
			}
			
			drawList.Colors.Add(color.ToVector4());
			drawList.Matrices.Add(Matrix4x4.CreateScale(scale)
			                      * Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
			                      * Matrix4x4.CreateTranslation(position));
			
			return drawList.Matrices.Count - 1;
		}
	}
}
