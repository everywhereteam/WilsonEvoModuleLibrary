using System.Collections.Generic;
using BlazorDynamicForm.Entities;
using WilsonEvoModuleLibrary.Common.Attributes;
using WilsonEvoModuleLibrary.Network;

namespace WilsonEvoModuleLibrary.Entities;

public class ModelsConfiguration
{
    public FormMap? ConfigurationScheme { get; set; }
    public Dictionary<string, TaskAttribute> Tasks { get; set; } = new();
    public List<NetworkNode> Network { get; set; } = new();
}