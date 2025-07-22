using System.Diagnostics;
using System.Reflection;
using Arch.Core;
using Cinenic.Extensions.CSharp;
using Cinenic.Resources;
using Schedulers;
using ComponentRegistry = Cinenic.Components.ComponentRegistry;

namespace Cinenic {
	
	public static class CINENIC {

		public static int UpdatesPerSecond { get; set; } = 100;
		
		public static Thread UpdateThread { get; private set; }
		public static bool IsRunning { get; set; }
		
		public static TimeSpan LastUpdate { get; private set; }
		public static TimeSpan LastRender { get; private set; }
		
		public static TimeSpan UpdateDelta { get; private set; }
		public static TimeSpan RenderDelta { get; private set; }

		private static Stopwatch _updateStopwatch = new();
		private static Stopwatch _renderStopwatch = new();

		private static JobScheduler _sharedWorldScheduler;
		
		public static void Run() {
			ComponentRegistry.AddAssembly(Assembly.GetExecutingAssembly());
			ComponentRegistry.RegisterComponents();
			
			IsRunning = true;
			
			UpdateThread = new Thread(_UpdateLoop);
			UpdateThread.Start();
			
			// render loop in main thread
			_RenderLoop();
		}

		public static void Stop() {
			IsRunning = false;
			UpdateThread.Join(); // wait for update thread to finish
		}

		private static void _UpdateLoop() {
			_sharedWorldScheduler = new JobScheduler(new JobScheduler.Config {
				ThreadPrefixName = "Cinenic.WJS",
				ThreadCount = 0,
				MaxExpectedConcurrentJobs = 32,
				StrictAllocationMode = false
			});
			
			World.SharedJobScheduler = _sharedWorldScheduler;
			
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
			
			_sharedWorldScheduler.Dispose();
		}

		private static void _RenderLoop() {
			_renderStopwatch.Restart();
			
			while(IsRunning) {
				var currentTime = _renderStopwatch.Elapsed;
				var sinceLastRender = currentTime - LastRender;

				RenderDelta = sinceLastRender;
				//UpdateDelta = sinceLastRender;
				ThreadScheduler.RunSchedules();
				RenderManager.Render(sinceLastRender);
				//UpdateManager.Update(sinceLastRender);
				LastRender = currentTime;
				//LastUpdate = currentTime;

				if(RenderPipelineManager.PipelineStates.All(kv => !kv.Value)) {
					// nothing left to render, so we quit
					Stop();
				}
			}
		}
	}
}
