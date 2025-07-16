using Cinenic.Renderer;

namespace Cinenic {
	
	public static class RenderManager {
		
		public static void Add(RenderQueue queue, params IRenderable[] renderables) {
			queue.Queue.AddRange(renderables);
		}
		
		public static void Add(IRenderable renderable, params RenderQueue[] queues) {
			foreach(var queue in queues) {
				queue.Queue.Add(renderable);
			}
		}
		
		public static void Add(RenderQueue[] queues, IRenderable[] renderables) {
			foreach(var queue in queues) {
				queue.Queue.AddRange(renderables);
			}
		}

		public static void ChangeQueue(RenderQueue from, RenderQueue to) {
			to.Queue.AddRange(from.Queue);
			from.Queue.Clear();
		}

		public static void ChangeQueue(RenderQueue from, RenderQueue to, params IRenderable[] renderables) {
			foreach(var renderable in renderables) {
				if(!from.Queue.Remove(renderable)) continue;
				to.Queue.Add(renderable);
			}
		}
		
		public static void Render(TimeSpan delta) {
			foreach(var pipeline in PipelineManager.Pipelines.Values) {
				if(!PipelineManager.IsEnabled(pipeline)) continue;
				Render(pipeline, delta);
			}
		}

		public static void Render(RenderPipeline pipeline, TimeSpan delta) {
			if(!PipelineManager.IsEnabled(pipeline)) return;

			if(!pipeline.Begin()) {
				PipelineManager.SetEnabled(pipeline, false);
				return;
			}
			
			foreach(var renderable in pipeline.Queue.Queue) {
				renderable.Render(pipeline.Queue, delta);
			}
			
			pipeline.End();
		}
	}
}
