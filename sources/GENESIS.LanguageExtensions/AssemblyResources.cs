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
			return assembly.ReadTextResource(assembly.GetName().Name + ".Resources." + name);
		}
		
		public static byte[] ReadBinaryResource(this Assembly assembly, string path) {
			using(var stream = assembly.GetManifestResourceStream(path))
			using(var memory = new MemoryStream()) {
				stream.CopyTo(memory);
				return memory.ToArray();
			}
		}
		
		public static byte[] ReadBinaryResourceN(this Assembly assembly, string name) {
			return assembly.ReadBinaryResource(assembly.GetName().Name + ".Resources." + name);
		}
	}
}
