﻿using System.ComponentModel.DataAnnotations;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Common.Task
{
    [Task("End", Categories.Flow)]
    public class EndTask : BaseTask
    {
        [Display(Name = "Message to user"), DataType(DataType.Text)]
        public string Message { get; set; }
    }
}
