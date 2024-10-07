using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services.Core;

public abstract class NodeServices<TN, TC> : IExecutionService, IEnvironmentDeploy<TN>, INodeServices<TN, TC>
    where TN : BaseTask where TC : class
{
    public Task Execute(object nodeData, SessionData data)
    {
        return Execute((TN)nodeData, data);
    }

    public abstract Task Execute( TN nodeData, SessionData data);

    public Task HandleDeployInternal(string projectCode, Dictionary<string, BaseTask> nodes)
    {
        return HandleDeploy(projectCode, nodes.ToDictionary(kvp => kvp.Key, kvp => (TN)kvp.Value));
    }

    public virtual Task HandleDeploy(string projectCode, Dictionary<string, TN> nodes)
    {
        return Task.CompletedTask;
    }
}