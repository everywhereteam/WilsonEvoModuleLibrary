using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;   
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Common.Task;
[Task("JScript", Categories.Advanced, "description", "https://github.com/everywhereteam", true, Output.Ok, Output.Error)]
public class ScriptTask : BaseTask
{                                                    

    public Dictionary<string, string> VarsReference { get; set; }

    [DataType(DataType.MultilineText), Display(Name = "Message"), DefaultValue("")]
    public string Script { get; set; }
}

