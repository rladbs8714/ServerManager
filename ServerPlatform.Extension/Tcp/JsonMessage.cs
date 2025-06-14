using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerPlatform.Extension
{
    public class JsonMessage
    {

        // ====================================================================
        // ENUMS
        // ====================================================================

        public enum EMessageType
        {
            None,
            Normal,
            Discord
        }


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("type")]
        public EMessageType Type { get; set; }


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public JsonMessage() { }

        public JsonMessage(string name, string message, EMessageType type)
        {
            Name = name;
            Message = message;
            Type = type;
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
            try
            {
                r = JsonSerializer.Deserialize<JsonMessage>(json);
            }
            catch
            {
                r = null;
            }
            

            return r != null;
        }
    }
}
