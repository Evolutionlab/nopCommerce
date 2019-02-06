using Newtonsoft.Json;

namespace Nop.Plugin.Evolutionlab.ExportFic.Models
{
    public class DocDettagliResponse : BaseRequest
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("dettagli_documento")]
        public DocRequest DettagliDocumento { get; set; }
    }
}
