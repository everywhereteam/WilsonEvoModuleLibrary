using System;
using BlazorDynamicFormGenerator;

namespace WilsonEvoModuleLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TaskProviderAttribute : Attribute
{
    public TaskProviderAttribute(string name)
    {
        ServiceName = name;
    }

    public string ServiceName { get; set; }

    public ModuleNodeDefinition Definition { get; set; }
}