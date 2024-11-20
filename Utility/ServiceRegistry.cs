using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Utility;

public class TaskRegistry
{

    public List<Type> Tasks = new();

    public void AddTask<ITask>()
    {
        Tasks.Add(typeof(ITask));
    }
}

public class ServiceRegistry(IServiceCollection _services)
{
    public Dictionary<(string taskType, string? channel), (Type taskType, Type executor, ExecutorType type)> ExecutorTypes = new();


    public void AddExecutor<Executor>(string? channel = null) where Executor : ITaskExecutor
    {
        var executorType = typeof(Executor);

        // Check if executor is already registered in services
        if (!_services.Any(sd => sd.ServiceType == executorType))
        {
            _services.AddTransient(executorType);
        }

        var executorKind = GetExecutorType(executorType);

        // Find the node type
        var nodeType = GetNodeTypeFromExecutor(executorType);

        if (nodeType == null)
        {
            throw new InvalidOperationException($"Cannot determine node type for executor {executorType.FullName}");
        }

        // Extract the generic type arguments
        ExecutorTypes[(nodeType.Name, channel)] = (nodeType, executorType, executorKind);
    }

    private ExecutorType GetExecutorType(Type executorType)
    {
        if (typeof(ISynchronousTaskExecutor).IsAssignableFrom(executorType))
        {
            return ExecutorType.Synchronous;
        }

        if (typeof(IAsynchronousTaskExecutor).IsAssignableFrom(executorType))
        {
            return ExecutorType.Asynchronous;
        }

        throw new InvalidOperationException($"Executor type {executorType.FullName} does not implement a recognized executor interface.");
    }

    private Type? GetNodeTypeFromExecutor(Type executorType)
    {
        // Traverse inheritance hierarchy to find base class and extract TTask
        var baseType = executorType.BaseType;

        while (baseType != null && baseType != typeof(object))
        {
            if (baseType.IsGenericType)
            {
                var genericDef = baseType.GetGenericTypeDefinition();

                if (genericDef == typeof(TaskExecutorBase<,>) || genericDef == typeof(AsyncNodeServices<,>))
                {
                    return baseType.GetGenericArguments()[0]; // TTask
                }
            }

            baseType = baseType.BaseType;
        }

        return null;
    }
}