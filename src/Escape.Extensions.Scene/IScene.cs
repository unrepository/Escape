using Arch.Core;

namespace Escape.Extensions.Scene {
	
	public interface IScene : IDisposable {

		public World AsWorld();
		public Entity Export(ref World world, Entity? parent);
	}
}
