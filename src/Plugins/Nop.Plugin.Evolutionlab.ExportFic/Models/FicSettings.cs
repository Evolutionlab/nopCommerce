using Nop.Core.Configuration;

namespace Nop.Plugin.Evolutionlab.ExportFic.Models
{
  public class FicSettings : ISettings
  {
    public string ApiUid { get; set; }

    public string ApiKey { get; set; }

    public string BaseUrlPost { get; set; }

    public string Numerazione { get; set; }

    public bool Attivo { get; set; }
    }
}
