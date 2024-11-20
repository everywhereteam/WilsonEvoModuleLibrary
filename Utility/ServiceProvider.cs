using System;
using System.Collections.Generic;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Utility;

public class ServiceProvider(ModuleConfiguration config, IServiceProvider _serviceProvider)
{
 
    public (Type, ITaskExecutor, ExecutorType) GetExecutor(string taskType, string? channel)
    {
        if (config.ExecutorTypes.TryGetValue((taskType, channel), out var executorType))
        {
            // Resolve a new instance from the service provider
            var executorInstance = (ITaskExecutor)_serviceProvider.GetService(executorType.executor);

            if (executorInstance == null)
            {
                throw new InvalidOperationException($"Cannot resolve executor of type {executorType.executor.FullName} from the service provider.");
            }

            return (executorType.taskType, executorInstance, executorType.type);
        }

        throw new InvalidOperationException($"Executor not found for task type {taskType} and channel '{channel}'.");
    }
}