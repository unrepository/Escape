using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Eclair.Renderer;
using Eclair.Renderer.Compute;
using Eclair.Renderer.Shader;
using Eclair.Extensions.CSharp;
using Eclair.Presentation;
using GENESIS.Project;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Framebuffer = Eclair.Renderer.Framebuffer;
using Shader = Eclair.Renderer.Shader.Shader;

namespace GENESIS.UI.Object {
	
	public class MapEditor : ImGuiScene {
		
		public MapObject Object { get; }
		
		public ShaderCompute TerrainCompute { get; }
		public MapRenderData TerrainRenderData = new();
		public IShaderData<MapRenderData> TerrainComputeData { get; }
		
		// TODO
		// 1. *split generation into own namespace
		// 2. implement simplex noise in ILGPU
		// 3. when zoomed out, generate noise for offset (0,0) for given (window) resolution
		// also dont generate every frame but only when offset changed
		// 4. when zoomed in, generate noise for zoomed offset (x,y) and at the same resolution
		// also: store position of map data such as markers as normalized coordinates [-1,1] in double
		public unsafe MapEditor(IPlatform platform, MapObject obj) : base(platform, "map_editor") {
			Object = obj;
			
			//MapFramebuffer = Framebuffer.Create(platform, new Vector2D<uint>(512, 512));

			TerrainCompute = new ShaderCompute(
				platform,
				ShaderProgram.Create(
					platform,
					Shader.Create(
						platform,
						ShaderType.FragmentShader,
						Assembly
							.GetExecutingAssembly()
							.ReadTextResourceN("Shaders.OpenGL.test2.frag")
					)
				),
				new Vector2D<uint>(512, 512)
			);

			TerrainComputeData = IShaderData.Create(
				platform,
				1,
				TerrainRenderData,
				(uint) sizeof(MapRenderData)
			);
		}
		
		protected override void Paint(double delta) {
			TerrainComputeData.Data = TerrainRenderData;
			TerrainComputeData.Push();
			TerrainCompute.Render();
			
			if(ImGui.Begin($"Map editor - {Object.ProjectPath}", ref IsOpen)) {
				ImGui.Image(
					new ImTextureID(TerrainCompute.Framebuffer.GetTextureAttachments()[0].Handle),
					new Vector2(TerrainCompute.Size.X, TerrainCompute.Size.Y),
					new Vector2(0, 1), new Vector2(1, 0)
				);

				ImGui.DragFloat("Zoom", ref TerrainRenderData.Zoom, 0.001f);
				ImGui.DragFloat("Offset X", ref TerrainRenderData.Offset.X, 0.001f);
				ImGui.DragFloat("Offset Y", ref TerrainRenderData.Offset.Y, 0.001f);
			}

			ImGui.End();
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct MapRenderData() {

			[FieldOffset(0)] public Vector2 Offset = Vector2.Zero;
			[FieldOffset(8)] public float Zoom = 1.0f;
			[FieldOffset(12)] private float _padding0 = 0;
		}
	}
}
