using WilsonEvoModuleLibrary.Attributes;         

namespace WilsonEvoModuleLibrary.Configurations;

[ServiceProviderConfiguration("CONFIGURATION : WHATSAPP")]
public class GupshupWhatsappConfiguration
{
    public string Number { get; set; }

    public string Alias { get; set; }

    public string CallbackUri { get; set; }

    public string LicenseKey { get; set; }

    public string MessagesMsecDelay { get; set; }

    public bool ListWithLessThenThreeFieldBecameQuickResponse { get; set; }
}