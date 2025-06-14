using System.Diagnostics;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using Silk.NET.OpenGL;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	public class GLPainter : Painter {

		private readonly GLPlatform _platform;

		public GLPainter(GLPlatform platform) : base(platform) {
			_platform = platform;
		}

		public override int BeginDrawList(DrawList.ShapeType type = DrawList.ShapeType.Triangle, bool instanced = false) {
			Debug.Assert(CurrentDrawList == -1, "BeginDrawList() called without EndDrawList()");
			
			CurrentDrawList = DrawLists.Count;
			if(instanced) DrawLists.Add(new GLInstancedDrawList(_platform, type));
			else DrawLists.Add(new GLDrawList(_platform, type));
			return CurrentDrawList;
		}
		
		public override void EndDrawList() {
			Debug.Assert(CurrentDrawList != -1, "EndDrawList() called without BeginDrawList()");
			CurrentDrawList = -1;
		}

		public override void Paint() {
			foreach(var drawList in DrawLists) {
				if(!drawList.IsEnabled) continue;
				
				drawList.Push();
				drawList.Draw();
			}
		}
	}
}
