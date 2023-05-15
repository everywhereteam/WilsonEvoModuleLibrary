using WilsonPluginInterface.Attributes.Property;
using WilsonPluginModels.Interfaces;

namespace WilsonPluginCommons.Nodes;

public class OutputProcessTask : INode
{
    [InputBox("Message", "Insert message..")]
    public string Message { get; set; }
}