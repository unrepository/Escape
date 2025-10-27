using System.Diagnostics;
using System.Reflection;
using Arch.Core;
using Escape.Extensions.CSharp;
using Escape.Resources;
using Schedulers;
using ComponentRegistry = Escape.Core.Components.ComponentRegistry;

namespace Escape.Core {
	
	// TODO separate update/fixedupdate cause forcing update to be threaded is sometimes funky
	public static class ESCAPE {

		public static string ProjectName { get; set; } = "Project";
		public static Assembly ProjectAssembly { get; set; }
		
		public static int UpdatesPerSecond { get; set; } = 100;
		
		public static Thread UpdateThread { get; private set; }
		public static bool IsRunning { get; set; }
		
		public static TimeSpan LastUpdate { get; private set; }
		public static TimeSpan LastRender { get; private set; }
		
		public static TimeSpan UpdateDelta { get; private set; }
		public static TimeSpan RenderDelta { get; private set; }

		private static Stopwatch _updateStopwatch = new();
		private static Stopwatch _renderStopwatch = new();
		
		public delegate void CleanupEventHandler();
		public static event CleanupEventHandler? OnCleanup;
		
		public static void Run() {
			Debug.Assert(ProjectAssembly is not null);
			
			Components.ComponentRegistry.AddAssembly(ProjectAssembly);
			Components.ComponentRegistry.RegisterComponents();
			
			IsRunning = true;
			
			// UpdateThread = new Thread(_UpdateLoop);
			// UpdateThread.Start();
			
			// render loop in main thread
			_RenderLoop();
		}

		public static void Stop() {
			IsRunning = false;
			//UpdateThread.Join(); // wait for update thread to finish
			WorldEngine.SharedJobScheduler.Dispose();
			
			OnCleanup?.Invoke();
		}

		private static void _UpdateLoop() {
			_updateStopwatch.Restart();
			
			while(IsRunning) {
				var currentTime = _updateStopwatch.Elapsed;
				var sinceLastUpdate = currentTime - LastUpdate;

				var targetDelta = TimeSpan.FromSeconds(1.0 / UpdatesPerSecond);

				if(sinceLastUpdate >= targetDelta) {
					UpdateDelta = sinceLastUpdate;
					ThreadScheduler.RunSchedules();
					UpdateManager.Update(sinceLastUpdate);
					LastUpdate = currentTime;
				} else {
					Thread.Yield();
					Thread.Sleep(1); // not completely accurate but CPU-friendly
				}
			}
		}

		private static void _RenderLoop() {
			_renderStopwatch.Restart();
			
			while(IsRunning) {
				var currentTime = _renderStopwatch.Elapsed;
				var sinceLastRender = currentTime - LastRender;

				RenderDelta = sinceLastRender;
				UpdateDelta = sinceLastRender;
				ThreadScheduler.RunSchedules();
				RenderManager.Render(sinceLastRender);
				UpdateManager.Update(sinceLastRender);
				LastUpdate = currentTime;
				LastRender = currentTime;

				if(RenderPipelineManager.PipelineStates.All(kv => !kv.Value)) {
					// nothing left to render, so we quit
					Stop();
				}
			}
		}
	}
}
