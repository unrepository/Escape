using System.Numerics;
using Arch.Core.Extensions;
using Escape;
using Escape.Components;
using Escape.Extensions.ImGui;
using Escape.Renderer;
using Hexa.NET.ImGui;
using static Shared;

public static class ImGuiTest {

	public unsafe static void Start(string[] args) {
		SetupVulkan(out var platform, out var shaderPipeline, out var renderQueue, out var renderPipeline);
		CreateWindow(platform, "ImGui Test", ref renderQueue, out var window);

		ImGuiController.Create(platform, "test", renderQueue, window);
		
		RenderManager.Add(renderQueue, "imgui test", (_, _) => {
			ImGui.ShowDemoWindow();
		});
		
		RenderManager.Add(renderQueue, "imgui test", (_, _) => {
			ImGui.Begin("Hello, world!");
			ImGui.Text("meow");
			ImGui.Button("wah");
			ImGui.End();
		});
	
		CreateWorld(platform, shaderPipeline, renderQueue, out var world);
		CreateOrbitalCamera(ref world, window, out var cameraEntity, out var orbitalCamera);
		orbitalCamera.Distance = 5;
		
		world.Create(
			new Transform3D(),
			new RenderableObject(CubeModel)
		);
		
		ESCAPE.Run();
	}
}
