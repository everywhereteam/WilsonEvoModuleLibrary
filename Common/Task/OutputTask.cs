using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("Output", Categories.Io, Output.Ok, Output.Error)]
public class OutputTask : BaseTask
{
    [Required]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Message")]
    [DefaultValue("")]
    public string Message { get; set; }
}