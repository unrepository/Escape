using System.Reflection;

namespace GENESIS.LanguageExtensions {
	
	public static class Resources {

		public static Stream Get(string name) {
			var assembly = Assembly.GetCallingAssembly();
			var path = assembly.GetName().Name + ".Resources." + name;

			var stream = assembly.GetManifestResourceStream(path);
			
			if(stream is null) {
				throw new FileNotFoundException("File does not exist in embedded resources", path);
			}

			return stream;
		}

		public static string LoadText(string name) {
			using var stream = Get(name);
			using var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}
		
		public static byte[] LoadBinary(string name) {
			using var stream = Get(name);
			using var memory = new MemoryStream();
			
			stream.CopyTo(memory);
			return memory.ToArray();
		}
	}
}
