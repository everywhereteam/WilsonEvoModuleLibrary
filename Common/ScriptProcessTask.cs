using System.Collections.Generic;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Interfaces;

namespace WilsonEvoModuleLibrary.CommonNodes;

[Task("Script", "Advanced", OutputDefault, OutputError)]
public class ScriptProcessTask : INode
{
    public const string OutputDefault = "default";
    public const string OutputError = "error";

    public Dictionary<string, string> VarsReference { get; set; }

    public string Script { get; set; }
}