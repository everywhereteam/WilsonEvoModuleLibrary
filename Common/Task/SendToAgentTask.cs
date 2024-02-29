using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Entities.Values;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("SendToAgent", Categories.Function, Output.Ok, Output.Error, Output.Timeout)]
public class SendToAgentTask : BaseTask
{
    [Display(Name = "Variables"), DataType("Dictionary")]
    public Dictionary<string, string> Data { get; set; } = new();

    [Display(Name = "Destination"), DataType(DataType.Text)]
    public string Destination { get; set; }

    [Display(Name = "Destination"), DataType(DataType.Text)]
    public string WaitingMessage { get; set; }

    [Display(Name = "Operator Response Timeout"), DataType(DataType.Text)]
    public int OperatorResponseTimeout { get; set; }
}