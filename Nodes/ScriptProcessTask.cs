using System.Collections.Generic;
using WilsonPluginInterface.Attributes;
using WilsonPluginInterface.Attributes.Property;
using WilsonPluginModels.Interfaces;

namespace WilsonPluginCommons.Nodes;

[TaskDefinition("Script", "Advanced", OutputDefault, OutputError)]
public class ScriptProcessTask : INode
{
    public const string OutputDefault = "default";
    public const string OutputError = "error";

    [DictionaryList("Input variables")] public Dictionary<string, string> VarsReference { get; set; }

    [InputCode("Script")] public string Script { get; set; }
}