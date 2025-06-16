using Eclair.Renderer.OpenGL;
using Eclair.Presentation;
using Eclair.Presentation.Extensions;
using GENESIS.UI;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Monitor = Silk.NET.Windowing.Monitor;
using Window = Eclair.Renderer.Window;

Silk.NET.Windowing.Window.PrioritizeGlfw();

var platformOptions = new GLPlatform.Options();
platformOptions.ParseCommandLine(args);

var platform = new GLPlatform(platformOptions);
var windowOptions = WindowOptions.Default;

var monitorCenter = Monitor.GetMainMonitor(null).Bounds.Center;
					
windowOptions.Position = new Vector2D<int>(
	monitorCenter.X - windowOptions.Size.X / 2,
	monitorCenter.Y - windowOptions.Size.Y / 2
);

windowOptions.WindowState = WindowState.Maximized;

var window = Window.Create(platform, windowOptions);

platform.Initialize();
window.Initialize();
			
var splash = new SplashScreen(platform);
window.PushScene(splash);

if(args.Contains("--debug")) {
	window.PushScene(new DebugScene(platform));
}

while(!window.Base.IsClosing) {
	window.RenderFrame();
}

window.PopAllScenes();
