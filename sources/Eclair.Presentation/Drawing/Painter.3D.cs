using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Eclair.Renderer;
using GENESIS.LanguageExtensions;

namespace Eclair.Presentation.Drawing {
	
	public abstract partial class Painter {
		
		public int Add3DCube(Vector3 position, Vector3 rotation, Vector3 scale, Material material)
			=> Add3DObject(Models.Cube, position, rotation, scale, material);

		public int Add3DObject(Model? model, Vector3 position, Vector3 rotation, Vector3 scale, Material? material = null) {
			Debug.Assert(CurrentDrawList != -1, "Add3DObject() called outside a draw list");
			var drawList = DrawLists[CurrentDrawList];
			Debug.Assert((model is null && drawList.IsInstanced) || (model is not null && !drawList.IsInstanced),
				"Model must be null in an instanced draw list and cannot be null in a non-instanced draw list");
			
			int c = model is null ? 1 : model.Meshes.Count;

			for(int i = 0; i < c; i++) {
				if(model is not null) {
					drawList.Meshes.Add(model.Meshes[i]);
					drawList.Textures.Add(model.Meshes[i].Textures);
				} else {
					drawList.Textures.Add([]);
				}
				
				if(material.HasValue) drawList.Materials.Add(material.Value);
				else drawList.Materials.Add(drawList.Meshes[i].Material);

				var matrix =
					Matrix4x4.CreateScale(scale)
					* Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
					* Matrix4x4.CreateTranslation(position);
				
				drawList.Matrices.Add(matrix);
			}
			
			return drawList.Matrices.Count - 1;
		}
	}
}
