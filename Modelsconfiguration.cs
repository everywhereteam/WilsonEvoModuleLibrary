using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorDynamicFormGenerator;

namespace WilsonEvoModuleLibrary
{
    public class Modelsconfiguration
    {
        public Dictionary<string, ModuleNodeDefinition> Definitions { get; set; } = new();
    }
}
