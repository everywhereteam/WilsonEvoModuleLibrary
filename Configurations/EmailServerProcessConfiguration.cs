using WilsonPluginInterface.Attributes;

namespace WilsonPluginCommons.Configurations;

[ServiceProviderConfiguration("Email server credentials")]
public class EmailServerProcessConfiguration
{
    public string Server { get; set; }

    public int Port { get; set; }

    public string Account { get; set; }

    public string Password { get; set; }

    public string SecurityType { get; set; }

    public string ProtocolType { get; set; }
}