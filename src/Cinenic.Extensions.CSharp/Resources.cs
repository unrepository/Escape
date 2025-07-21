using System.Reflection;

namespace Cinenic.Extensions.CSharp {
	
	public static class Resources {

		[Obsolete("Use the resource system")]
		public static Stream Get(string name, Assembly? assembly = null) {
			assembly ??= Assembly.GetCallingAssembly();
			var path = assembly.GetName().Name + ".Resources." + name;

			var stream = assembly.GetManifestResourceStream(path);
			
			if(stream is null) {
				throw new FileNotFoundException("File does not exist in embedded resources", path);
			}

			return stream;
		}

		[Obsolete("Use the resource system")]
		public static string LoadText(string name) {
			using var stream = Get(name, Assembly.GetCallingAssembly());
			using var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}
		
		[Obsolete("Use the resource system")]
		public static byte[] LoadBinary(string name) {
			using var stream = Get(name, Assembly.GetCallingAssembly());
			using var memory = new MemoryStream();
			
			stream.CopyTo(memory);
			return memory.ToArray();
		}
	}
}
