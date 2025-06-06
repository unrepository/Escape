using System.Diagnostics;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	public class GLPainter3D : Painter3D {

		public override void BeginDrawList() {
			Debug.Assert(CurrentDrawList == -1, "BeginDrawList() called without EndDrawList()");

			CurrentDrawList = DrawLists.Count;
			DrawLists.Add(new GLDrawList());
		}
		
		public override void EndDrawList() {
			Debug.Assert(CurrentDrawList != -1, "EndDrawList() called without BeginDrawList()");
			CurrentDrawList = -1;
		}
	}
}
