using System.Collections.Generic;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Network;

namespace WilsonEvoModuleLibrary.Entities
{
    public class ModelsConfiguration
    {
        public TaskProviderAttribute TaskProvider { get; set; }
        public Dictionary<string, TaskAttribute> Tasks { get; set; } 
        public NetworkDefinition Network { get; set; }
                              
        public ModelsConfiguration() { }
    }
}                                                                             