using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Cinenic.Components;

namespace Cinenic.Systems {
	
	public partial class MainUpdateSystem : BaseSystem<World, TimeSpan> {

		public MainUpdateSystem(World world) : base(world) { }
	}
}
