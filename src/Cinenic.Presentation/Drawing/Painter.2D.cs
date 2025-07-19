using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Cinenic.Renderer;
using Cinenic.Extensions.CSharp;

namespace Cinenic.Presentation.Drawing {
	
	public abstract partial class Painter {

		public int Add2DQuad(Vector2 position, float rotation, Vector2 scale, Material material)
			=> Add2DObject(Models.Quad, position, rotation, scale, material);

		public int Add2DObject(Model? model, Vector2 position, float rotation, Vector2 scale, Material? material = null) {
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
				
				// if(material.HasValue) drawList.Materials.Add(material.Value);
				// else drawList.Materials.Add(drawList.Meshes[i].Material);

				var matrix =
					Matrix4x4.CreateScale(new Vector3(scale.X, scale.Y, 1))
					* Matrix4x4.CreateFromYawPitchRoll(0, 0, rotation)
					* Matrix4x4.CreateTranslation(new Vector3(position.X, position.Y, 0));
				
				drawList.Matrices.Add(matrix);
			}
			
			return drawList.Matrices.Count - 1;
		}
	}
}
