using System.Collections.Generic;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Attributes.Property;
using WilsonEvoModuleLibrary.Interfaces;

namespace WilsonEvoModuleLibrary.Nodes;

[TaskDefinition("Script", "Advanced", OutputDefault, OutputError)]
public class ScriptProcessTask : INode
{
    public const string OutputDefault = "default";
    public const string OutputError = "error";

    [DictionaryList("Input variables")] public Dictionary<string, string> VarsReference { get; set; }

    [InputCode("Script")] public string Script { get; set; }
}