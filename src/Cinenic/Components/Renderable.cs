using Cinenic.Renderer;

namespace Cinenic.Components {

	// TODO actually implement this?
	public record struct DynamicRenderable(Func<TimeSpan, Model> Render);
	
	[Component]
	public record struct Renderable(Model Model);
}
