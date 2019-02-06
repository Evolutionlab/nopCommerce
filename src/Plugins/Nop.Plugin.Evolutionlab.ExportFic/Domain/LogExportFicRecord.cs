using System;
using Nop.Core;

namespace Nop.Plugin.Evolutionlab.ExportFic.Domain
{
    /// <summary>
    /// Rappresenta il log dell'esportazione in Fatture in cloud
    /// </summary>
    public partial class LogExportFicRecord : BaseEntity
    {
        public DateTime Data { get; set; }

        public string Tipo { get; set; }

        public int OrdineId { get; set; }

        public string OrdineFatInCloud { get; set; }

        public string DdtFatInCloud { get; set; }

        public int OrdineNumero { get; set; }

        public bool Errore { get; set; }

        public string Token { get; set; }

        public string Messaggio { get; set; }

        public string MessaggioDDT { get; set; }

        public double TotaleDb { get; set; }

        public double TotaleEsportato { get; set; }
    }
}