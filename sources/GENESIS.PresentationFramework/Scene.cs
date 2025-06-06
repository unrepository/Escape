using GENESIS.GPU;
using GENESIS.PresentationFramework.Drawing;

namespace GENESIS.PresentationFramework {
	
	public abstract class Scene {
		
		public Painter Painter { get; }
		public string Id { get; }

		protected Scene(Painter painter, string id) {
			Painter = painter;
			Id = id;
		}

		public virtual void Initialize(Window window) {}
		public virtual void Deinitialize(Window window) {}

		public virtual void Render(double delta) {
			Paint(delta);
		}

		protected abstract void Paint(double delta);
	}
}
