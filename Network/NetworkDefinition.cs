using System.Collections.Generic;
using MessagePack;

namespace WilsonEvoModuleLibrary.Network;
                                               
public class NetworkDefinition
{           
    public List<NetworkNode> Network { get; set; } = new();
}