using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerPlatform.Extension
{
    public class JsonMessageForNormal : JsonMessage
    {

        // ====================================================================
        // PROPERTIES
        // ====================================================================

        [JsonPropertyName("guid")]
        public Guid Guid { get; set; }


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public JsonMessageForNormal(string name, string message, Guid guid) : base(name, message)
        {
            Guid = guid;
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        public override string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static bool TryParse(string json, out JsonMessageForNormal? r)
        {
            r = JsonSerializer.Deserialize<JsonMessageForNormal>(json);

            return r != null;
        }
    }
}
