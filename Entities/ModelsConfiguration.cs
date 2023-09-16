using System.Collections.Generic;
using WilsonEvoModuleLibrary.Attributes;

namespace WilsonEvoModuleLibrary.Entities
{
    public class ModelsConfiguration
    {
        public TaskProviderAttribute? TaskProvider { get; set; }
        public Dictionary<string, TaskAttribute> Tasks { get; set; } = new();
    }
}                                                                             