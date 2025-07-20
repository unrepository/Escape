using Cinenic.Renderer;

namespace Cinenic.World.Components {

	// TODO actually implement this?
	public record struct DynamicRenderable(Func<TimeSpan, Model> Render);
	
	public record struct Renderable(Model Model);
}
