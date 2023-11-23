using System.Collections.Generic;

namespace WilsonEvoModuleLibrary.Network;

public class NetworkDefinition
{
    //public Dictionary<string, string> Network { get; set; } = new();
    public List<NetworkNode> Network { get; set; } = new();
}

public class NetworkNode
{
    public string TaskTypeName { get; set; }
    public string TaskTypeFullName { get; set; }
    public string? ChannelControllerTypeName { get; set; }
    public string? ChannelControllerTypeFullName { get; set; }
}