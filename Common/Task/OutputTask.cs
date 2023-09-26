
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Xml.Linq;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Common.Values;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("Output", Categories.IO, Output.Ok, Output.Error)]
public class OutputTask : BaseTask
{
    [Required, DataType(DataType.MultilineText), Display(Name = "Message"), DefaultValue("")]
    public string Message { get; set; }
}