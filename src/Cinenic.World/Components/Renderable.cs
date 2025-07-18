using Cinenic.Renderer;

namespace Cinenic.World.Components {

	public record struct Renderable(Func<TimeSpan, RenderableModel> Render);
}
