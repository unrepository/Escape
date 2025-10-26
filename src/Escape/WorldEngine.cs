using Arch.Core;
using Escape.Renderer;
using Schedulers;

namespace Escape {
	
	public class WorldEngine {

		public static World? World {
			get => field;
			set {
				if(_worldUpdater is not null) UpdateManager.Remove(_worldUpdater.Id);
				if(_worldRenderer is not null && _renderQueue is not null) RenderManager.Remove(_renderQueue, _worldRenderer);

				if(value is not null) {
					_worldUpdater = new WorldUpdater("main_engine", value);
					UpdateManager.Add(_worldUpdater);

					if(_renderQueue is not null) {
						_worldRenderer = new WorldRenderer(
							"main_engine",
							value,
							ObjectRenderer.Create(_renderQueue.Platform, _renderQueue.Pipeline!.ShaderPipeline)
						);
						
						RenderManager.Add(_renderQueue, _worldRenderer);
					}
				}

				field = value;
			}
		}

		public static JobScheduler SharedJobScheduler;

		private static RenderQueue? _renderQueue;
		
		private static WorldUpdater? _worldUpdater;
		private static WorldRenderer? _worldRenderer;

		static WorldEngine() {
			SharedJobScheduler = new JobScheduler(new JobScheduler.Config {
				ThreadPrefixName = "Main World Job Scheduler",
				ThreadCount = 0,
				MaxExpectedConcurrentJobs = 32,
				StrictAllocationMode = false
			});

			World.SharedJobScheduler = SharedJobScheduler;
		}
		
		public static void Configure(IPlatform platform, RenderQueue renderQueue) {
			if(_worldRenderer is not null && _renderQueue is not null) {
				RenderManager.Remove(_renderQueue, _worldRenderer);
			}

			_renderQueue = renderQueue;

			if(World is not null) {
				_worldRenderer = new WorldRenderer(
					"main_engine",
					World,
					ObjectRenderer.Create(platform, renderQueue.Pipeline!.ShaderPipeline)
				);
				
				RenderManager.Add(renderQueue, _worldRenderer);
			}
		}
	}
}
