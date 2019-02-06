using Newtonsoft.Json;

namespace Nop.Plugin.Evolutionlab.ExportFic.Models
{
    public class BaseRequest
    {
        [JsonProperty("api_uid")]
        public string ApiUid { get; set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }
    }
}
