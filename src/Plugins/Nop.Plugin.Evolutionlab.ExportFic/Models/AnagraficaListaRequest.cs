using Newtonsoft.Json;

namespace Nop.Plugin.Evolutionlab.ExportFic.Models
{
    public class AnagraficaListaRequest : BaseRequest
    {
        [JsonProperty("nome")]
        public string Nome { get; set; }

        [JsonProperty("cf")]
        public string Cf { get; set; }

        [JsonProperty("piva")]
        public string Piva { get; set; }
    }
}
