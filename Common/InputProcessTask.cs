using WilsonEvoModuleLibrary.Attributes;

namespace WilsonEvoModuleLibrary.Common;

[Task("Input Intent", "Categoria.INTENT", "NotFound", "MaxExc", "ok", "error")]
public class InputProcessTask
{
    public string MessageConfirmation { get; set; }

    public string MessageDisambiguation { get; set; }

    public string Variable { get; set; }
}