﻿using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Common.Attributes;
using WilsonEvoModuleLibrary.Common.Entities.Values;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Common.Task;

[Task("End", Categories.Flow)]
public class EndTask : BaseTask
{
    [Display(Name = "Reason(debug)"), DataType(DataType.Text)]
    public string Reason { get; set; }

    [Display(Name = "Message to user"), DataType(DataType.Text)]
    public string Message { get; set; }
}