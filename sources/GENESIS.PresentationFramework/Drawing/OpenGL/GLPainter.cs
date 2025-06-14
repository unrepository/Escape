using System.Diagnostics;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using GENESIS.LanguageExtensions;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ARB;
using Shader = GENESIS.GPU.Shader.Shader;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	public class GLPainter : Painter {

		private readonly GLPlatform _platform;
		private readonly ArbBindlessTexture _bindlessTexture;

		public GLPainter(GLPlatform platform) : base(platform) {
			_platform = platform;
			_bindlessTexture = new(_platform.API.Context);
		}

		public override int BeginDrawList(DrawList.ShapeType type = DrawList.ShapeType.Triangle) {
			Debug.Assert(CurrentDrawList == -1, "BeginDrawList() called without EndDrawList()");
			
			CurrentDrawList = DrawLists.Count;
			DrawLists.Add(new GLDrawList(_platform, type));
			return CurrentDrawList;
		}
		
		public override void EndDrawList() {
			Debug.Assert(CurrentDrawList != -1, "EndDrawList() called without BeginDrawList()");
			
			CurrentDrawList = -1;
			CurrentModel = null;
		}

		public override void Paint() {
			foreach(var drawList in DrawLists) {
				if(!drawList.Enabled) continue;
				
				drawList.Push();

				foreach(var (i, texture) in drawList.Textures.Enumerate()) {
					texture?.Bind(i);
				}
				
				_platform.API.DrawArraysInstanced(
					((GLDrawList) drawList).GLShapeType,
					0,
					(uint) drawList.Vertices.Count,
					(uint) drawList.Matrices.Count
				);
			}
		}
	}
}
