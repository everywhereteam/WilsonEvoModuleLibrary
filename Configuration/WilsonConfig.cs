namespace WilsonEvoModuleLibrary.Configuration;

public class WilsonConfig
{
    public string Token { get; set; }
    public bool IsDebug { get; set; } = false;
    public string ChannelName { get; set; }
    public string ServiceBus { get; set; }
}