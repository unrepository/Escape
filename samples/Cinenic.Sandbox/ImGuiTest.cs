using System.Numerics;
using Arch.Core.Extensions;
using Cinenic;
using Cinenic.Components;
using Cinenic.Extensions.ImGui;
using Cinenic.Renderer;
using Cinenic.Renderer.Shader.Pipelines;
using Cinenic.Renderer.Vulkan;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.Vulkan;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using static Shared;
using VkDevice = Cinenic.Renderer.Vulkan.VkDevice;

public static class ImGuiTest {

	public unsafe static void Start(string[] args) {
		var platform = new VkPlatform();
		platform.Initialize();

		platform.PrimaryDevice = platform.CreateDevice(0);
		
		// SetupVulkan(out var platform, out var shaderPipeline, out var renderQueue, out var renderPipeline);
		// CreateWindow(platform, "GLTF Test", ref renderQueue, out var window);

		var window = Window.Create(platform);
		window.Title = "ImGui test";
		
		UpdateManager.Add((WindowUpdater) window);
		
		var controller = new VkImGuiController(platform);
		
		window.Initialize(controller.Queue);
		controller.Initialize(window);
		
		RenderManager.Add(controller.Queue, "dumb shit", (queue, delta) => {
			ImGui.ShowDemoWindow();
		});
	
		//CreateWorld(platform, shaderPipeline, renderQueue, out var world);
		//CreateOrbitalCamera(ref world, window, out var cameraEntity, out var orbitalCamera);
		// orbitalCamera.Distance = 5;
		//
		// world.Create(
		// 	new Transform3D(),
		// 	new RenderableObject(CubeModel)
		// );
		
		CINENIC.Run();
	}
}
