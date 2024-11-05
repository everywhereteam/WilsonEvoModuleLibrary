using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;
using WilsonEvoModuleLibrary.Services.Core;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("End", Categories.Flow)]
public class EndTask : ITask
{
    [Display(Name = "Message to user"), DataType(DataType.Text)]
    public string Message { get; set; }
}