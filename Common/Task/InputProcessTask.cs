using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("Input", Categories.IO, Output.Ok, Output.Error, Output.Timeout)]
public class InputTask : BaseTask
{
    [Display(Name = "Message Confirmation"), DataType(DataType.Text)]
    public string MessageConfirmation { get; set; }
    [Display(Name = "Message Disambiguation"), DataType(DataType.Text)]
    public string MessageDisambiguation { get; set; }
    [Display(Name = "Variable"), DataType(DataType.Text)]
    public string Variable { get; set; }
}