using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core;
using ArchRegistry = Arch.Core.ComponentRegistry;

namespace Cinenic.Components {
	
	// for Arch AOT
	public static class ComponentRegistry {

		private static readonly List<Assembly> _assemblies = [];

		public static void AddAssembly(Assembly assembly) {
			_assemblies.Add(assembly);
		}

		public static void RegisterComponents() {
			RegisterComponents<ComponentAttribute>();
		}
		
		// TODO move this reflection thingy to a common extension thing so it can be used also e.g. in Renderer.Resources.ResourceRegistry
		private static void RegisterComponents<TAttribute>()
			where TAttribute : Attribute
		{
			foreach(var assembly in _assemblies) {
				foreach(var type in assembly.GetTypes()) {
					if(!type.IsValueType && !type.IsClass) continue;
					if(type.GetCustomAttributes(typeof(TAttribute), inherit: false).Length == 0) continue;

					RegisterComponent(type);
				}
			}
		}
		
		private static void RegisterComponent(Type type) {
			var size = (int?)
				typeof(Unsafe)
				.GetMethod(nameof(Unsafe.SizeOf), BindingFlags.Public | BindingFlags.Static)?
				.MakeGenericMethod(type)
				.Invoke(null, null);

			if(size is null) return;
			
			var id = ArchRegistry.Size + 1;
			var componentType = Activator.CreateInstance(typeof(ComponentType), id, size);
			
			typeof(ArchRegistry)
				.GetMethod("Add", BindingFlags.Public | BindingFlags.Static, [ typeof(Type), typeof(ComponentType) ])?
				.Invoke(null, [ type, componentType ]);

			typeof(ArrayRegistry)
				.GetMethod("Add", BindingFlags.Public | BindingFlags.Static)?
				.MakeGenericMethod(type)
				.Invoke(null, null);
		}
	}
	
	public class ComponentAttribute : Attribute { }
}
