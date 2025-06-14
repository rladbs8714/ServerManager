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

        public JsonMessageForNormal() { }

        public JsonMessageForNormal(string name, string message, EMessageType type, Guid guid) : base(name, message, type)
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
            try
            {
                r = JsonSerializer.Deserialize<JsonMessageForNormal>(json);
            }
            catch
            {
                r = null;
            }
            

            return r != null;
        }
    }
}
