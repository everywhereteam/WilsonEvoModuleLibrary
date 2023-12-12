using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("SendToAgent", Categories.Function, Output.Ok, Output.Error, Output.Timeout)]
public class SendToAgentTask
{
    [Display(Name = "Variables")]
    [DataType("Dictionary")]
    public Dictionary<string, string> Data { get; set; } = new();
}