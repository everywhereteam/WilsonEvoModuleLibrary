using BlazorDynamicFormGenerator;
using System;

namespace WilsonEvoModuleLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TaskAttribute : Attribute
{
    public TaskAttribute(string name, string category, string cssIcon = "", string description = "", params string[] defaultOutputs)
    {
        Name = name;
        Category = category;
        DefaultOutputs = defaultOutputs;
        CssIcon = cssIcon;
        Description = description;

    }

    public string Name { get; set; }
    public string Category { get; set; }
    public string[] DefaultOutputs { get; set; }
    public string Description { get; set; }
    public string CssIcon { get; set; }

    public ModuleNodeDefinition Definition { get; set; }
}