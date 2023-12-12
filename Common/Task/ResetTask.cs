using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Common.Values;

namespace WilsonEvoModuleLibrary.Common.Task
{
    [Task("Reset", Categories.Flow, Output.Ok, Output.Error)]
    public class ResetTask
    {
        [Display(Name = "Reason(debug)"), DataType(DataType.Text)]
        public string Reason { get; set; }
    }
}
