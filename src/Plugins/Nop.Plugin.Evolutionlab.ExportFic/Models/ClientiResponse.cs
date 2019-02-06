using Newtonsoft.Json;

namespace Nop.Plugin.Evolutionlab.ExportFic.Models
{
    public partial class ClientiResponse : BaseRequest
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("nome", NullValueHandling = NullValueHandling.Ignore)]
        public string Nome { get; set; }

        [JsonProperty("referente", NullValueHandling = NullValueHandling.Ignore)]
        public string Referente { get; set; }

        [JsonProperty("indirizzo_via", NullValueHandling = NullValueHandling.Ignore)]
        public string IndirizzoVia { get; set; }

        [JsonProperty("indirizzo_cap", NullValueHandling = NullValueHandling.Ignore)]
        public string IndirizzoCap { get; set; }

        [JsonProperty("indirizzo_citta", NullValueHandling = NullValueHandling.Ignore)]
        public string IndirizzoCitta { get; set; }

        [JsonProperty("indirizzo_provincia", NullValueHandling = NullValueHandling.Ignore)]
        public string IndirizzoProvincia { get; set; }

        [JsonProperty("indirizzo_extra", NullValueHandling = NullValueHandling.Ignore)]
        public string IndirizzoExtra { get; set; }

        [JsonProperty("paese", NullValueHandling = NullValueHandling.Ignore)]
        public string Paese { get; set; }

        [JsonProperty("mail", NullValueHandling = NullValueHandling.Ignore)]
        public string Mail { get; set; }

        [JsonProperty("tel", NullValueHandling = NullValueHandling.Ignore)]
        public string Tel { get; set; }

        [JsonProperty("fax", NullValueHandling = NullValueHandling.Ignore)]
        public string Fax { get; set; }

        [JsonProperty("piva", NullValueHandling = NullValueHandling.Ignore)]
        public string Piva { get; set; }

        [JsonProperty("cf", NullValueHandling = NullValueHandling.Ignore)]
        public string Cf { get; set; }

        [JsonProperty("termini_pagamento", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? TerminiPagamento { get; set; }

        [JsonProperty("pagamento_fine_mese", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PagamentoFineMese { get; set; }

        [JsonProperty("val_iva_default", NullValueHandling = NullValueHandling.Ignore)]
        public double ValIvaDefault { get; set; }

        [JsonProperty("desc_iva_default", NullValueHandling = NullValueHandling.Ignore)]
        public string DescIvaDefault { get; set; }

        [JsonProperty("extra", NullValueHandling = NullValueHandling.Ignore)]
        public string Extra { get; set; }

        [JsonProperty("PA")]
        public bool Pa { get; set; }

        [JsonProperty("PA_codice", NullValueHandling = NullValueHandling.Ignore)]
        public string PaCodice { get; set; }
    }
}
