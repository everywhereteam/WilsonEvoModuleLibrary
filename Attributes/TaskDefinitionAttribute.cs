using System;

namespace WilsonPluginInterface.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TaskDefinitionAttribute : Attribute
{
    public TaskDefinitionAttribute(string taskName, string category, params string[] defaultOutputs)
    {
        TaskName = taskName;
        Category = category;
        DefaultOutputs = defaultOutputs;
    }

    public string TaskName { get; set; }
    public string Category { get; set; }
    public string[] DefaultOutputs { get; set; }
}