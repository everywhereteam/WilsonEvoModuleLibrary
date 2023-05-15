using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Attributes.Property;
using WilsonEvoModuleLibrary.Interfaces;

namespace WilsonEvoModuleLibrary.Nodes;

[TaskDefinition("Input Intent", "Categoria.INTENT", "NotFound", "MaxExc", "ok", "error")]
public class InputProcessTask : INode
{
    [InputBox("Confirm message",
        "Message prompted to user if a confirmation is required: {0} will be replaced by supposed intent name")]
    public string MessageConfirmation { get; set; }

    [InputBox("Message Disambiguation",
        "Message prompt to user if a disambiguation is required {0},{1},{2} will be replaced by supposed intent names")]
    public string MessageDisambiguation { get; set; }

    [SelectVar(typeof(string), "Tua mamma", "puttana")]
    public string Variable { get; set; }
}