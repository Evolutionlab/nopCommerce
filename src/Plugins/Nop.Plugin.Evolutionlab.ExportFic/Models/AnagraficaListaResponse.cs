using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Evolutionlab.ExportFic.Models
{
    public class AnagraficaListaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("lista_clienti")]
        public List<ClientiResponse> ListaClienti { get; set; }

        [JsonProperty("pagina_corrente")]
        public int PaginaCorrente { get; set; }

        [JsonProperty("numero_pagine")]
        public int NumeroPagine { get; set; }
    }
}
