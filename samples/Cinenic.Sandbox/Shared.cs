using System.Drawing;
using Cinenic;
using Cinenic.Renderer;
using Cinenic.Renderer.Shader.Pipelines;
using Cinenic.Renderer.Vulkan;
using Cinenic.World;
using Flecs.NET.Core;

public static class Shared {
	
	public static readonly Model CubeModel = new Model {
        Meshes = [
        	new Mesh {
        		Vertices = [
        			new() { Position = new(-0.5f,  0.5f,  0.5f), UV = new(0, 1) },
        			new() { Position = new(-0.5f, -0.5f,  0.5f), UV = new(0, 0) },
        			new() { Position = new( 0.5f, -0.5f,  0.5f), UV = new(1, 0) },
        			new() { Position = new( 0.5f,  0.5f,  0.5f), UV = new(1, 1) },
        			new() { Position = new( 0.5f,  0.5f,  0.5f), UV = new(0, 1) },
        			new() { Position = new( 0.5f, -0.5f,  0.5f), UV = new(0, 0) },
        			new() { Position = new( 0.5f, -0.5f, -0.5f), UV = new(1, 0) },
        			new() { Position = new( 0.5f,  0.5f, -0.5f), UV = new(1, 1) },
        			new() { Position = new( 0.5f,  0.5f, -0.5f), UV = new(0, 1) },
        			new() { Position = new( 0.5f, -0.5f, -0.5f), UV = new(0, 0) },
        			new() { Position = new(-0.5f, -0.5f, -0.5f), UV = new(1, 0) },
        			new() { Position = new(-0.5f,  0.5f, -0.5f), UV = new(1, 1) },
        			new() { Position = new(-0.5f,  0.5f, -0.5f), UV = new(0, 1) },
        			new() { Position = new(-0.5f, -0.5f, -0.5f), UV = new(0, 0) },
        			new() { Position = new(-0.5f, -0.5f,  0.5f), UV = new(1, 0) },
        			new() { Position = new(-0.5f,  0.5f,  0.5f), UV = new(1, 1) },
        			new() { Position = new(-0.5f,  0.5f, -0.5f), UV = new(0, 0) },
        			new() { Position = new(-0.5f,  0.5f,  0.5f), UV = new(0, 1) },
        			new() { Position = new( 0.5f,  0.5f,  0.5f), UV = new(1, 1) },
        			new() { Position = new( 0.5f,  0.5f, -0.5f), UV = new(1, 0) },
        			new() { Position = new(-0.5f, -0.5f,  0.5f), UV = new(0, 0) },
        			new() { Position = new(-0.5f, -0.5f, -0.5f), UV = new(0, 1) },
        			new() { Position = new( 0.5f, -0.5f, -0.5f), UV = new(1, 1) },
        			new() { Position = new( 0.5f, -0.5f,  0.5f), UV = new(1, 0) },
        		],
        		Indices = [
        			0, 1, 2, 0, 2, 3,
        			4, 5, 6, 4, 6, 7,
        			8, 9,10, 8,10,11,
        			12, 13, 14, 12, 14, 15,
        			16, 17, 18, 16, 18, 19,
        			20, 21, 22, 20, 22, 23
        		],
        		Material = new Material {
        			AlbedoColor = Color.White
        		}
        	}
        ]
    };

	public static void SetupVulkan(
		out VkPlatform platform,
		out DefaultSceneShaderPipeline shaderPipeline,
		out RenderQueue renderQueue,
		out RenderPipeline renderPipeline
	) {
		platform = new VkPlatform();
		platform.Initialize();

		platform.PrimaryDevice = platform.CreateDevice(0);
		
		shaderPipeline = new DefaultSceneShaderPipeline(platform);
		renderQueue = RenderQueueManager.Create(platform, "main");
		renderPipeline = RenderPipelineManager.Create(platform, "main", renderQueue, shaderPipeline);
	}

	public static void CreateWindow(
		IPlatform platform,
		string title,
		ref RenderQueue renderQueue,
		out Window window
	) {
		window = Window.Create(platform);
		window.Title = title;
		window.Initialize(renderQueue);
		renderQueue.RenderTarget = window.Framebuffer;
		
		UpdateManager.Add((WindowUpdater) window);
	}

	public static void CreateWorld(
		IPlatform platform,
		DefaultSceneShaderPipeline shaderPipeline,
		RenderQueue renderQueue,
		out World world
	) {
		world = World.Create();
		
		UpdateManager.Add(new WorldUpdater("world", world));

		var objectRenderer = ObjectRenderer.Create(platform, shaderPipeline);
		RenderManager.Add(new WorldRenderer("world", objectRenderer, world), renderQueue);
		RenderManager.Add(objectRenderer, renderQueue);
	}
}
