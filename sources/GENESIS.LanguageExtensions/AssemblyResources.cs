using System.Diagnostics;
using System.Reflection;

namespace GENESIS.LanguageExtensions {
	
	public static class AssemblyResources {

		public static string ReadTextResource(this Assembly assembly, string path) {
			using(var stream = assembly.GetManifestResourceStream(path))
			using(var reader = new StreamReader(stream)) {
				return reader.ReadToEnd();
			}
		}
		
		public static string ReadTextResourceN(this Assembly assembly, string name) {
			name = assembly.GetName().Name + ".Resources." + name;
			
			using(var stream = assembly.GetManifestResourceStream(name))
			using(var reader = new StreamReader(stream)) {
				return reader.ReadToEnd();
			}
		}
	}
}
