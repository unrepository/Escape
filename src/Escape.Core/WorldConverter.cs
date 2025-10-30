using System.Text.Json;
using System.Text.Json.Serialization;
using Arch.Core;

namespace Escape.Core {
	
	public class WorldConverter : JsonConverter<World> {

		public override World? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			//var serializer = new ArchJsonSerializer();
			//using var jsonDocument = JsonDocument.ParseValue(ref reader);

			//return serializer.FromJson(jsonDocument.RootElement.GetRawText());
			return null;
		}
		
		public override void Write(Utf8JsonWriter writer, World value, JsonSerializerOptions options) {
			//var serializer = new ArchJsonSerializer();
			//writer.WriteRawValue(serializer.ToJson(value));
		}
	}
}
