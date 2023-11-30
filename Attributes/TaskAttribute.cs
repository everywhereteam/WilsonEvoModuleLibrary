using System;
using BlazorDynamicFormGenerator;
using MessagePack;
using MessagePack.Formatters;

namespace WilsonEvoModuleLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TaskAttribute : Attribute
{
    public TaskAttribute(string name, string category, string cssIcon, string cssNode, string description,
        string documentationUrl,
        bool hasDynamicOutput, params string[] defaultOutputs)
    {
        Name = name;
        Category = category;
        DefaultOutputs = defaultOutputs;
        CssIcon = cssIcon;
        Description = description;
        HasDynamicOutput = hasDynamicOutput;
        DocumentationUrl = documentationUrl;
        CssNode = cssNode;
    }

    public TaskAttribute(string name, string category, string description, string documentationUrl,
        bool hasDynamicOutput, params string[] defaultOutputs)
    {
        Name = name;
        Category = category;
        DefaultOutputs = defaultOutputs;
        Description = description;
        HasDynamicOutput = hasDynamicOutput;
        DocumentationUrl = documentationUrl;
    }

    public TaskAttribute(string name, string category, params string[] defaultOutputs)
    {
        Name = name;
        Category = category;
        DefaultOutputs = defaultOutputs;
    }

    public TaskAttribute()
    {
    }
    public string Name { get; set; }

    public string Category { get; set; }

    public string[] DefaultOutputs { get; set; }

    public string Description { get; set; }

    public string CssIcon { get; set; }

    public string CssNode { get; set; }

    public string DocumentationUrl { get; set; }

    public bool HasDynamicOutput { get; set; }
    public ModuleNodeDefinition? Definition { get; set; } = null;
}