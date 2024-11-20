using System.Collections.Generic;
using WilsonEvoModuleLibrary.Attributes;
using WilsonEvoModuleLibrary.Network;

namespace WilsonEvoModuleLibrary.Entities;

public class ModelsConfiguration
{
    public TaskProviderAttribute? TaskProvider { get; set; }
    //public Dictionary<string, TaskAttribute>? Tasks { get; set; }
    public NetworkDefinition? Network { get; set; }

    public List<TaskDefinition> Tasks { get; set; } = new();
    public List<ProcessorDefinition> Processors { get; set; } = new();
    public List<Channel> Channels { get; set; } = new();
}

public class Channel
{
    public string Name { get; set; }
}

public class TaskDefinition
{
    public string Type { get; set; }
    public string Name { get; set; }
    public byte[] RawConfiguration { get; set; } = new byte[]{};
}

public class ProcessorDefinition
{
    public string TaskType { get; set; }
    public string? ChannelName { get; set; }
    public string Type { get; set; }
}