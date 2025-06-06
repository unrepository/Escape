using System.Diagnostics;
using System.Reflection;

namespace GENESIS.LanguageExtensions {
	
	public static class AssemblyResources {

		public static string ReadTextResource(this Assembly assembly, string name) {
			using(var stream = assembly.GetManifestResourceStream(name))
			using(var reader = new StreamReader(stream)) {
				return reader.ReadToEnd();
			}
		}
	}
}
