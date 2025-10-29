using System.Reflection;
using Escape.Core;
using Escape.Renderer;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Shader.Pipelines;
using Silk.NET.Windowing;
using Spectre.Console.Cli;
using Window = Escape.Renderer.Window;

namespace Escape.Editors.ResourceEditor {
	
	public sealed class Command : Command<Command.Settings> {

		public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken) {
			ESCAPE.ProjectName = "ResourceEditor";
			ESCAPE.ProjectAssembly = Assembly.GetExecutingAssembly();
			
			var platform = new GLPlatform();
			platform.Initialize();

			var shaderPipeline = new DefaultUnshadedShaderPipeline(platform);
			var renderQueue = RenderQueueManager.Create(platform, "main");
			renderQueue.ClearColor = Color.Transparent;
			var renderPipeline = RenderPipelineManager.Create(platform, "main", renderQueue, shaderPipeline);

			var window = Window.Create(platform, WindowOptions.Default with {
				TransparentFramebuffer = true
			});
			window.Title = "Resource Editor";
			window.Initialize(renderQueue);
			
			renderQueue.RenderTarget = window.Framebuffer;

			EditorGlobals.SingleWindow = true;
			SceneEngine.SetScene(renderQueue, new EditorScene(platform, renderQueue));
			ESCAPE.Run();

			return 0;
		}

		public sealed class Settings : CommandSettings {
			
		}
	}
}
