using System.Diagnostics;
using GENESIS.GPU;
using GENESIS.PresentationFramework.Drawing;

namespace GENESIS.PresentationFramework {
	
	public abstract class Scene {
		
		public Painter Painter { get; protected set; }
		public string Id { get; }
		
		public Window? Window { get; private set; }
		public double UpdatesPerSecond { get; init; } = 60;
		
		protected Thread UpdateThread { get; private set; }
		
		private bool _runUpdateThread = true;

		protected Scene(Painter painter, string id) {
			Painter = painter;
			Id = id;
		}

		public virtual void Initialize(Window window) {
			Window = window;
			
			UpdateThread = new Thread(() => {
				double interval = 1000.0 / UpdatesPerSecond;

				var stopwatch = new Stopwatch();
				stopwatch.Start();

				long nextTick = stopwatch.ElapsedTicks;
				long lastTick = nextTick;
				long ticksPerInterval = (long) (Stopwatch.Frequency / UpdatesPerSecond);

				while(_runUpdateThread) {
					nextTick += ticksPerInterval;

					while(stopwatch.ElapsedTicks < nextTick) {
						long remainingTicks = nextTick - stopwatch.ElapsedTicks;

						if(remainingTicks > Stopwatch.Frequency / 1000) {
							Thread.Sleep(1);
						} else {
							Thread.SpinWait(2);
						}
					}

					long currentTick = stopwatch.ElapsedTicks;
					long deltaTicks = currentTick - lastTick;
					lastTick = currentTick;

					double delta = (double) deltaTicks / Stopwatch.Frequency;
					Update(delta);
				}
			}) {
				IsBackground = true
			};
			UpdateThread.Start();
		}
		
		public virtual void Deinitialize(Window window) {
			_runUpdateThread = false;
			UpdateThread.Join();

			Window = null;
		}

		public virtual void Update(double delta) {}
		
		public virtual void Render(double delta) {
			Paint(delta);
		}

		protected abstract void Paint(double delta);
	}
}
