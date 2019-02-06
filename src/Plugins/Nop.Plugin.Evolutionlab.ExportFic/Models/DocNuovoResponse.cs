using Newtonsoft.Json;

namespace Nop.Plugin.Evolutionlab.ExportFic.Models
{
    public class DocNuovoResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("new_id")]
        public int NewOrderId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
