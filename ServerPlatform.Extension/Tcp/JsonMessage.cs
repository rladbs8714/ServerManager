using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerPlatform.Extension
{
    public class JsonMessage
    {
        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public JsonMessage(string name, string message)
        {
            Name = name;
            Message = message;
        }


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public virtual string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static bool TryParse(string json, out JsonMessage? r)
        {
            r = JsonSerializer.Deserialize<JsonMessage>(json);

            return r != null;
        }
    }
}
