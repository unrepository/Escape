using System.Text.Json;
using System.Text.Json.Serialization;
using Arch.Core;
using Arch.Core.Extensions;

namespace Escape.Core {
	
	public class WorldConverter : JsonConverter<World> {

		public static JsonSerializerOptions SerializerOptions { get; set; } = new() {
			IndentCharacter = '\t',
			IndentSize = 1,
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			ReadCommentHandling = JsonCommentHandling.Skip,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		private static readonly EntityConverter _entityConverter = new EntityConverter() {
			World = default
		};
		
		public override World? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			var w = World.Create();
			_entityConverter.World = w;
			
			var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			foreach(var entity in root.GetProperty("entities").EnumerateArray()) {
				_entityConverter.Read(ref reader, typeof(Entity), options);
			}

			return w;
		}
		
		public override void Write(Utf8JsonWriter writer, World value, JsonSerializerOptions options) {
			writer.WriteStartObject();
			{
				writer.WriteNumber("root_entity", value.GetRootEntity().Id);
				
				writer.WriteStartArray("entities");
				
				foreach(var entity in value.GetEntities()) {
					_entityConverter.Write(writer, entity, SerializerOptions);
				}
				
				writer.WriteEndArray();
			}
			writer.WriteEndObject();
		}
	}
}
