       using System;
using System.Collections.Generic;
using System.Linq;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Utility;

public class ModuleConfiguration
{
    public string Token { get; set; }

    public ModuleLoader.AzureBusSettings? AzureBusSettings { get; set; }

    public Type? ModuleConfigurationType { get; set; }

    public Dictionary<(string taskType, string? channel), (Type taskType, Type executor, ExecutorType type)> ExecutorTypes { get; set; } = new();
    
    public Dictionary<Type, Type> DeploymentHandlers { get; set; } = new();

    public TaskRegistry TaskRegistry { get; set; } = new();

    public ModelsConfiguration GetConfiguration()
    {
        var config = new ModelsConfiguration();
        config.Channels = ExecutorTypes
            .Select(x => x.Key.channel)
            .Distinct()
            .Select(x=>new Channel(){Name = x})
            .ToList();


        config.Processors = ExecutorTypes
            .Select(x => new ProcessorDefinition() { TaskType = x.Key.taskType, ChannelName = x.Key.channel })
            .ToList();
                                                                                         //missing definition
        config.Tasks = TaskRegistry.Tasks.Select(x => new TaskDefinition() { Type = x.Name }).ToList();
        return config;
    }
}
