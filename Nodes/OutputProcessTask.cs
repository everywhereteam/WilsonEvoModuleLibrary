using WilsonEvoModuleLibrary.Attributes.Property;
using WilsonEvoModuleLibrary.Interfaces;

namespace WilsonEvoModuleLibrary.Nodes;

public class OutputProcessTask : INode
{
    [InputBox("Message", "Insert message..")]
    public string Message { get; set; }
}