using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerPlatform.Extension
{
    public class JsonMessageForDiscord : JsonMessage
    {
        // ====================================================================
        // JSON PROPERTIES
        // ====================================================================

        /// <summary>
        /// 명령 id
        /// </summary>
        [JsonPropertyName("id")]
        public ulong ID { get; set; }
        /// <summary>
        /// 옵션
        /// </summary>
        [JsonPropertyName("options")]
        public string Options { get; set; }
        /// <summary>
        /// 옵션 구분자
        /// </summary>
        [JsonPropertyName("opt_sp")]
        public string OptionSeparator { get; set; }


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public JsonMessageForDiscord(string name, string message, ulong id, string option, string optSp) : base(name, message)
        {
            ID = id;
            Options = option;
            OptionSeparator = optSp;
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        public override string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static bool TryParse(string json, out JsonMessageForDiscord? r)
        {
            r = JsonSerializer.Deserialize<JsonMessageForDiscord>(json);

            return r != null;
        }
    }
}
