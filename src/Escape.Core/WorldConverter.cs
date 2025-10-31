using System.Text.Json;
using System.Text.Json.Serialization;
using Arch.Core;

namespace Escape.Core {
	
	// TODO
	public class WorldConverter : JsonConverter<World> {

		public override World? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			return null;
		}
		
		public override void Write(Utf8JsonWriter writer, World value, JsonSerializerOptions options) {
			
		}
	}
}
