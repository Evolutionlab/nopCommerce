using Newtonsoft.Json;

namespace Nop.Plugin.Evolutionlab.ExportFic.Models
{
    public class DocDettagliRequest : BaseRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("token ")]
        public string Token { get; set; }
    }
}
