using Arch.Core;

namespace Visio.Extensions.Scene {
	
	public interface IScene : IDisposable {

		public World AsWorld();
		public Entity Export(ref World world, Entity? parent);
	}
}
