using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services.Core;

public abstract class AsyncNodeServices<TN, TC> : IAsyncExecutionService, IEnvironmentDeploy<TN>, IAsyncNodeServices<TN, TC>
    where TN : BaseTask where TC : class
{
    public Task Execute(object nodeData, SessionData sessionData)
    {
        return Execute((TN)nodeData, sessionData);
    }

    public Task ExecuteCallback(object nodeData, SessionData sessionData)
    {
        return ExecuteCallback((TN)nodeData, sessionData);
    }

    public abstract Task Execute(TN nodeData, SessionData data);

    public abstract Task ExecuteCallback(TN nodeData, SessionData sessionData);

    public Task HandleDeployInternal(string projectCode, Dictionary<string, BaseTask> nodes)
    {
        return HandleDeploy(projectCode, nodes.ToDictionary(kvp => kvp.Key, kvp => (TN)kvp.Value));
    }

    public virtual Task HandleDeploy(string projectCode, Dictionary<string, TN> nodes)
    {
        return Task.CompletedTask;
    }
}