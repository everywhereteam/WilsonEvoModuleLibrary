using System.Collections.Generic;
using BlazorDynamicForm.Attributes.Display;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;
using WilsonEvoModuleLibrary.Services.Core;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("SendToAgent", Categories.Function, Output.Ok, Output.Error, Output.Timeout)]
public class SendToAgentTask : ITask
{
    [DisplayNameForm("Variables")] public Dictionary<string, string> Data { get; set; } = new();

    [DisplayNameForm("Destination")] public string Destination { get; set; }

    [DisplayNameForm("Destination")] public string WaitingMessage { get; set; }

    [DisplayNameForm("Operator Response Timeout")]
    public int OperatorResponseTimeout { get; set; }
}