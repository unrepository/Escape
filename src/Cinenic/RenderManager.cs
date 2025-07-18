using Cinenic.Renderer;

namespace Cinenic {
	
	public static class RenderManager {
		
		public static void Add(RenderQueue queue, params IRenderer[] renderers) {
			foreach(var renderer in renderers) {
				queue.Enqueue(renderer);
			}
		}
		
		public static void Add(IRenderer renderer, params RenderQueue[] queues) {
			foreach(var queue in queues) {
				queue.Enqueue(renderer);
			}
		}
		
		public static void Add(RenderQueue[] queues, IRenderer[] renderers) {
			foreach(var queue in queues) {
				Add(queue, renderers);
			}
		}

		public static void ChangeQueue(RenderQueue from, RenderQueue to) {
			foreach(var renderers in from.GetQueue().Values) {
				foreach(var renderer in renderers) {
					to.Enqueue(renderer);
				}
			}
			
			from.ClearQueue();
		}

		public static void ChangeQueue(RenderQueue from, RenderQueue to, params IRenderer[] renderers) {
			foreach(var renderer in renderers) {
				if(!from.Dequeue(renderer)) continue;
				to.Enqueue(renderer);
			}
		}
		
		public static void Render(TimeSpan delta) {
			foreach(var pipeline in RenderPipelineManager.Pipelines.Values) {
				if(!RenderPipelineManager.IsEnabled(pipeline)) continue;
				Render(pipeline, delta);
			}
		}

		public static void Render(RenderPipeline pipeline, TimeSpan delta) {
			if(!RenderPipelineManager.IsEnabled(pipeline)) return;

			if(!pipeline.Begin()) {
				RenderPipelineManager.SetEnabled(pipeline, false);
				return;
			}
			
			pipeline.Queue.Render(delta);
			pipeline.End();
		}
	}
}
