using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WilsonEvoCoreLibrary.Core.Models;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services;

public static class ExecutorInvoker
{
    private static readonly ConcurrentDictionary<Type, Func<ITaskExecutor, object, ProcessSession, object, Task>> _executeCache = new();
    private static readonly ConcurrentDictionary<Type, Func<ITaskExecutor, object, ProcessSession, object, Task>> _executeCallbackCache = new();

    public static Task InvokeExecuteAsync(ITaskExecutor executor, object nodeData, ProcessSession session, object configuration)
    {
        var executorType = executor.GetType();

        var invoker = _executeCache.GetOrAdd(executorType, type => CreateInvoker(type, "Execute"));

        return invoker(executor, nodeData, session, configuration);
    }

    public static Task InvokeExecuteCallbackAsync(ITaskExecutor executor, object nodeData, ProcessSession session, object configuration)
    {
        var executorType = executor.GetType();

        // Get or create the compiled delegate for the ExecuteCallback method
        var invoker = _executeCallbackCache.GetOrAdd(executorType, type => CreateInvoker(type, "ExecuteCallback"));

        // Invoke the method
        return invoker(executor, nodeData, session, configuration);
    }

    private static Func<ITaskExecutor, object, ProcessSession, object, Task> CreateInvoker(Type executorType, string methodName)
    {
        var methodInfo = executorType.GetMethod(methodName);

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Execute method not found in {executorType.FullName}");
        }

        var executorParam = Expression.Parameter(typeof(ITaskExecutor), "executor");
        var nodeDataParam = Expression.Parameter(typeof(object), "nodeData");
        var sessionParam = Expression.Parameter(typeof(ProcessSession), "session");
        var configurationParam = Expression.Parameter(typeof(object), "configuration");

        var castedExecutor = Expression.Convert(executorParam, executorType);
        var parameters = methodInfo.GetParameters();

        var castedNodeData = Expression.Convert(nodeDataParam, parameters[0].ParameterType);
        var castedConfiguration = Expression.Convert(configurationParam, parameters[2].ParameterType);

        var call = Expression.Call(castedExecutor, methodInfo, castedNodeData, sessionParam, castedConfiguration);

        return Expression.Lambda<Func<ITaskExecutor, object, ProcessSession, object, Task>>(call, executorParam, nodeDataParam, sessionParam, configurationParam).Compile();
    }
}