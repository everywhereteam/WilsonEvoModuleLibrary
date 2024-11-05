using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;
using WilsonEvoModuleLibrary.Services.Core;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("Reset", Categories.Flow, Output.Ok, Output.Error)]
public class ResetTask : ITask
{
    [Display(Name = "Reason(debug)"), DataType(DataType.Text)]
    public string Reason { get; set; }
}