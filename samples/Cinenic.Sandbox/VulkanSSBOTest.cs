using System.Numerics;
using Cinenic;
using Cinenic.Extensions.CSharp;
using Cinenic.Renderer;
using Cinenic.Renderer.Resources;
using Cinenic.Renderer.Shader;
using Cinenic.Renderer.Shader.Pipelines;
using Cinenic.Renderer.Vulkan;
using Cinenic.World;
using Cinenic.World.Components;
using Flecs.NET.Core;
using NLog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using Shader = Cinenic.Renderer.Shader.Shader;
using Window = Cinenic.Renderer.Window;

class SSBOTestShaderPipeline : IShaderPipeline {

	public IPlatform Platform { get; }
	public Cinenic.Resources.Ref<ShaderProgramResource> Program { get; }

	public DescriptorSet VkTexturesDescriptor { get; }

	public SSBOTestShaderPipeline(VkPlatform platform) {
		Platform = platform;
		
		// Program = ShaderProgram.Create(
		// 	platform,
		// 	Shader.Create(platform, Shader.Family.Vertex, Resources.LoadText("Shaders.vk3.vert")),
		// 	Shader.Create(platform, Shader.Family.Fragment, Resources.LoadText("Shaders.vk.frag"))
		// );
	}
	
	public void VkBindTextureUnit(uint unit, ImageView imageView, Silk.NET.Vulkan.Sampler sampler) { }
		
	public void PushData() { }
	public void Dispose() { }
}

public static class VulkanSSBOTest {
	
	private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

	public static void Start(string[] args) {
		var platform = new VkPlatform(new VkPlatform.Options {
			Headless = false
		});
		platform.Initialize();

		foreach(var device in platform.GetDevices()) {
			_logger.Info("Detected device: {name}", device.Name);
		}
		
		var primaryDevice = platform.CreateDevice(0);
		platform.PrimaryDevice = primaryDevice;
		_logger.Info("Using primary device 0 ({name})", primaryDevice.Name);
		
		_logger.Info("Create queues");
		var queue1 = RenderQueueManager.Create(platform, "world");
		
		_logger.Info("Create shader pipeline");
		var mainShaderPipeline = new SSBOTestShaderPipeline(platform);
		
		_logger.Info("Create buffers");
		var positions = new Vector2[] {
			new(0.0f, -0.5f),
			new(0.5f, 0.5f),
			new(-0.5f, 0.5f)
		};

		// need vec4 for aligning
		var colors = new Vector4[] {
			new(1, 0, 0, 0),
			new(0, 1, 0, 0),
			new(0, 0, 1, 0)
		};

		var positionData = IShaderArrayData.Create(platform, mainShaderPipeline.Program.Get(), 0, positions);
		var colorData = IShaderArrayData.Create(platform, mainShaderPipeline.Program.Get(), 1, colors);
		
		_logger.Info("Initial shader data push");
		positionData.Push();
		colorData.Push();
		
		_logger.Info("Create pipelines");
		var pipeline1 = RenderPipelineManager.Create(platform, "main", queue1, mainShaderPipeline);
		
		_logger.Info("Create window");
		var window = Window.Create(platform, WindowOptions.DefaultVulkan);
		window.Title = "Sandbox";
		window.Initialize(queue1);
		queue1.RenderTarget = window.Framebuffer;
		
		UpdateManager.Add((WindowUpdater) window);
		
		_logger.Info("Create world");
		using var world = World.Create();

		var triangle =
			world
				.Entity("triangle")
				.Set(new DynamicRenderable(delta => { platform.API.CmdDraw(((VkRenderQueue) pipeline1.Queue).CommandBuffer, 3, 1, 0, 0); return null; }));
		
		// var wr = new WorldRenderer("world", world);
		// RenderManager.Add(queue1, wr);
		
		_logger.Info("Begin loop");
		CINENIC.Run();
	}
}
