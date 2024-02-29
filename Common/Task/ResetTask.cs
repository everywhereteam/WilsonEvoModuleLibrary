using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Entities.Values;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("Reset", Categories.Flow, Output.Ok, Output.Error)]
public class ResetTask : BaseTask
{
    [Display(Name = "Reason(debug)"), DataType(DataType.Text)]
    public string Reason { get; set; }
}