using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Arch.Core;
using Arch.Core.Extensions;

namespace Escape.Core {
	
	public class EntityConverter : JsonConverter<Entity> {
		
		public required World? World { get; set; }

		public override Entity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			Debug.Assert(World is not null);
			
			var e = this.World.Create();

			var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			foreach(var component in root.GetProperty("components").EnumerateArray()) {
				var type = Type.GetType(component.GetProperty("type").GetString());
				var componentObj = component.GetProperty("value").Deserialize(type, options);
				
				e.Add(componentObj);
			}

			return e;
		}

		public override void Write(Utf8JsonWriter writer, Entity value, JsonSerializerOptions options) {
			writer.WriteStartObject();
			writer.WriteNumber("id", value.Id);
			writer.WriteNumber("world_id", value.WorldId);
			
			writer.WriteStartArray("components");
			
			foreach(var component in value.GetAllComponents()) {
				if(component is null) continue;
						
				writer.WriteStartObject();
				writer.WriteString("type", component.GetType().FullName);
				
				writer.WritePropertyName("value");
				writer.WriteRawValue(JsonSerializer.Serialize(component, options));
				
				writer.WriteEndObject();
			}
			
			writer.WriteEndArray();
			
			writer.WriteEndObject();
		}
	}
}
