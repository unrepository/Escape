using System.Reflection;
using Escape.Core;
using Escape.Editor;
using Escape.Editor.Scenes;
using Escape.Extensions.UI;
using Escape.Renderer;
using Escape.Renderer.OpenGL;
using Escape.Renderer.Shader.Pipelines;
using NLog;
using Silk.NET.Windowing;
using Window = Escape.Renderer.Window;

var logger = LogManager.GetCurrentClassLogger();

ESCAPE.ProjectName = "Escape Editor";
ESCAPE.ProjectAssembly = Assembly.GetExecutingAssembly();

logger.Info("Initializing platform");

var platform = new GLPlatform();
platform.Initialize();

logger.Info("Loading resources");
Resources.Load(platform);

logger.Info("Initializing pipeline");

var shaderPipeline = new DefaultUnshadedShaderPipeline(platform);
var renderQueue = RenderQueueManager.Create(platform, "main");
renderQueue.ClearColor = Color.Transparent;
var renderPipeline = RenderPipelineManager.Create(platform, "main", renderQueue, shaderPipeline);

var window = Window.Create(platform, WindowOptions.Default with {
	TransparentFramebuffer = true
});
window.Title = "Escape Editor";
window.Initialize(renderQueue);

renderQueue.RenderTarget = window.Framebuffer;

EditorGlobals.ImGuiController = ImGuiController.Create(platform, "main", renderQueue, window);

SceneEngine.SetScene(renderQueue, new Escape.Editor.Scenes.ProjectManager(platform, renderQueue));
ESCAPE.Run();
