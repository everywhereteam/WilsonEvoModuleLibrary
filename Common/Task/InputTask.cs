using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Entities.Values;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("Input", Categories.Io, Output.Ok, Output.Error, Output.Timeout)]
public class InputTask : BaseTask
{
    [Display(Name = "Message"), DataType(DataType.Text)]
    public string Message { get; set; }

    [Display(Name = "Variable"), DataType(DataType.Text)]
    public string Variable { get; set; }
}