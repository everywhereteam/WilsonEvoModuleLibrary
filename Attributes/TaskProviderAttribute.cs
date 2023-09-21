using System;
using BlazorDynamicFormGenerator;

namespace WilsonEvoModuleLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TaskProviderAttribute : Attribute
{
    public TaskProviderAttribute()
    {                           
    }

    public ModuleNodeDefinition Definition { get; set; }
}