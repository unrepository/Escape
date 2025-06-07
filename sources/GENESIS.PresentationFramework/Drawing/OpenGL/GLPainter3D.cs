using System.Diagnostics;
using GENESIS.GPU.OpenGL;
using GENESIS.GPU.Shader;
using Silk.NET.OpenGL;

namespace GENESIS.PresentationFramework.Drawing.OpenGL {
	
	public class GLPainter3D : Painter3D {

		private GLPlatform _platform;
		private IShaderProgram _targetShaderProgram;
		private IShader _targetShader;

		public GLPainter3D(GLPlatform platform, IShaderProgram targetShaderProgram, IShader targetShader) {
			_platform = platform;
			_targetShaderProgram = targetShaderProgram;
			_targetShader = targetShader;
		}

		public override void BeginDrawList() {
			Debug.Assert(CurrentDrawList == -1, "BeginDrawList() called without EndDrawList()");

			CurrentDrawList = DrawLists.Count;
			DrawLists.Add(new GLDrawList());
		}
		
		public override void EndDrawList() {
			Debug.Assert(CurrentDrawList != -1, "EndDrawList() called without BeginDrawList()");
			CurrentDrawList = -1;
			CurrentModel = null;
		}

		public override void Paint() {
			foreach(var drawList in DrawLists) {
				if(!drawList.Enabled) continue;
				_targetShaderProgram.Bind();
				
				drawList.Push(_targetShader);
				_platform.API.DrawArraysInstanced(
					GLEnum.Triangles,
					0,
					(uint) drawList.Vertices.Count,
					(uint) drawList.Matrices.Count
				);
			}
		}
	}
}
