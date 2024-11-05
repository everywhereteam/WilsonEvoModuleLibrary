       using System;
using System.Collections.Generic;
using WilsonEvoModuleLibrary.Entities;

public class ModuleConfiguration
{
    public string Token { get; set; }

    public Type? ModuleConfigurationType { get; set; }

    public Dictionary<(string taskType, string? channel), (Type taskType, Type executor, ExecutorType type)> ExecutorTypes { get; set; } = new();
    
    public Dictionary<Type, Type> DeploymentHandlers { get; set; } = new();
}
