using Cinenic.Extensions.CSharp;
using Cinenic.Renderer;
using Cinenic.Renderer.Shader;
using Cinenic.Renderer.Vulkan;
using NLog;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using Shader = Cinenic.Renderer.Shader.Shader;
using Window = Cinenic.Renderer.Window;

namespace Cinenic.Sandbox {
	
	public static class VulkanSandbox {
		
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

			_logger.Info("Create render queue");
			var queue = new VkRenderQueue(platform, RenderQueue.Family.Graphics, RenderQueue.Format.R8G8B8A8Srgb);
			//queue.Viewport = new Vector4D<int>(0, 0, 640, 480);
			//queue.Scissor = new Vector4D<int>(0, 0, 640, 480);
			queue.CreateAttachment();
			queue.CreateSubpass(
				0,
				ImageLayout.ColorAttachmentOptimal,
				new SubpassDescription {
					ColorAttachmentCount = 1
				},
				new SubpassDependency {
					SrcSubpass = Vk.SubpassExternal,
					DstSubpass = 0,
					SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
					SrcAccessMask = 0,
					DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
					DstAccessMask = AccessFlags.ColorAttachmentWriteBit
				}
			);
			queue.Initialize();
			
			_logger.Info("Create pipeline");
			var pipeline1 = new VkRenderPipeline(platform, queue, ShaderProgram.Create(
				platform,
				Shader.Create(platform, Shader.Family.Vertex, Resources.LoadText("Shaders.vk.vert")),
				Shader.Create(platform, Shader.Family.Fragment, Resources.LoadText("Shaders.vk.frag"))
			));
			
			var pipeline2 = new VkRenderPipeline(platform, queue, ShaderProgram.Create(
				platform,
				Shader.Create(platform, Shader.Family.Vertex, Resources.LoadText("Shaders.vk2.vert")),
				Shader.Create(platform, Shader.Family.Fragment, Resources.LoadText("Shaders.vk.frag"))
			));
			
			_logger.Info("Create window");
			var window = Window.Create(platform, pipeline1, WindowOptions.DefaultVulkan);
			window.Title = "Sandbox";
			
			window.Base.Render += delta => {
				window.Base.MakeCurrent();
				queue.Viewport = new Vector4D<int>(0, 0, window.Base.FramebufferSize.X, window.Base.FramebufferSize.Y);
				queue.Scissor = queue.Viewport;
				window.Pipeline.Begin(ref window.Framebuffer);
				platform.API.CmdDraw(queue.CommandBuffer, 3, 1, 0, 0);
				window.Pipeline.End(ref window.Framebuffer);

				if(window.Base.Time % 1 <= delta) {
					if(window.Pipeline == pipeline1) window.Pipeline = pipeline2;
					else window.Pipeline = pipeline1;
				}
			};
			
			_logger.Info("Begin rendering");
			while(!window.Base.IsClosing) {
				window.Base.DoEvents();
				if(!window.Base.IsClosing) window.Base.DoUpdate();
				if(window.Base.IsClosing) return;
				window.Base.DoRender();
			}
		}
	}
}
