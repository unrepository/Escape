using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using GENESIS.GPU;
using GENESIS.LanguageExtensions;

namespace GENESIS.PresentationFramework.Drawing {
	
	public abstract partial class Painter {
		
		public int Add2DQuad(Vector2 position, float rotation, Vector2 scale, Color color) {
			Debug.Assert(CurrentDrawList != -1, "Add2DQuad() called outside a draw list");
			Debug.Assert(CurrentModel is null or "quad2d", "Only a single model can be drawn per draw list");

			var drawList = DrawLists[CurrentDrawList];

			if(CurrentModel is null) {
				CurrentModel = "quad2d";
				drawList.Model = CurrentModel;
				drawList.Vertices.AddRange(Models.Quad);
			}
			
			drawList.Materials.Add(new Material { Albedo = color.ToVector4() });
			drawList.Matrices.Add(Matrix4x4.CreateScale(new Vector3(scale.X, scale.Y, 1))
			                      * Matrix4x4.CreateFromYawPitchRoll(0, 0, rotation)
			                      * Matrix4x4.CreateTranslation(new Vector3(position.X, position.Y, 0)));
			
			return drawList.Matrices.Count - 1;
		}

		public int Add2DObject(string modelName, Vector2 position, float rotation, Vector2 scale, Color color) {
			Debug.Assert(CurrentDrawList != -1, "Add2DObject() called outside a draw list");
			Debug.Assert(CurrentModel is null || CurrentModel == modelName, "Only a single model can be drawn per draw list");

			var drawList = DrawLists[CurrentDrawList];
			
			Debug.Assert(CustomModels.ContainsKey(modelName));

			if(CurrentModel is null) {
				CurrentModel = modelName;
				drawList.Model = CurrentModel;
				drawList.Vertices.AddRange(CustomModels[modelName]);
			}
			
			drawList.Materials.Add(new Material { Albedo = color.ToVector4() });
			drawList.Matrices.Add(Matrix4x4.CreateScale(new Vector3(scale.X, scale.Y, 1))
			                      * Matrix4x4.CreateFromYawPitchRoll(0, 0, rotation)
			                      * Matrix4x4.CreateTranslation(new Vector3(position.X, position.Y, 0)));
			
			return drawList.Matrices.Count - 1;
		}
	}
}
