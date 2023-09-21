using System.Collections.Generic;
using WilsonEvoModuleLibrary.Attributes;

namespace WilsonEvoModuleLibrary.Common;

[Task("Script", "Advanced", OutputDefault, OutputError)]
public class ScriptProcessTask 
{
    public const string OutputDefault = "default";
    public const string OutputError = "error";

    public Dictionary<string, string> VarsReference { get; set; }

    public string Script { get; set; }
}