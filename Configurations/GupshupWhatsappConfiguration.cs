using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Attributes.Property;

namespace WilsonEvoModuleLibrary.Configurations;

[ServiceProviderConfiguration("CONFIGURATION : WHATSAPP")]
public class GupshupWhatsappConfiguration
{
    [InputBox("Number")] public string Number { get; set; }

    [InputBox("Application Name")] public string Alias { get; set; }

    [InputBox("Callback URI")] public string CallbackUri { get; set; }

    [InputBox("License KEY")] public string LicenseKey { get; set; }

    [InputBox("Messages Msec Delay (1000 if empty)")]
    public string MessagesMsecDelay { get; set; }

    [Checkbox("Transform list with less then four fields in quick response")]
    public bool ListWithLessThenThreeFieldBecameQuickResponse { get; set; }
}