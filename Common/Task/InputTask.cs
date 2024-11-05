using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;
using WilsonEvoModuleLibrary.Services.Core;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("Input", Categories.Io, Output.Ok, Output.Error, Output.Timeout)]
public class InputTask : ITask
{
    [Display(Name = "Message"), DataType(DataType.Text)]
    public string Message { get; set; }

    [Display(Name = "Variable"), DataType(DataType.Text)]
    public string Variable { get; set; }
}