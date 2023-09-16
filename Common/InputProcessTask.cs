using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Interfaces;

namespace WilsonEvoModuleLibrary.CommonNodes;

[Task("Input Intent", "Categoria.INTENT", "NotFound", "MaxExc", "ok", "error")]
public class InputProcessTask : INode
{
    public string MessageConfirmation { get; set; }

    public string MessageDisambiguation { get; set; }

    public string Variable { get; set; }
}