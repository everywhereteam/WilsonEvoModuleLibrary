using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services.Core;

public abstract class NodeService<TN> : IExecutionService, IEnvironmentDeploy<TN>, INodeService<TN>, INodeService where TN : BaseTask
{
    public Task HandleDeployInternal(string projectCode, Dictionary<string, BaseTask> nodes)
    {
        return HandleDeploy(projectCode, nodes.ToDictionary(kvp => kvp.Key, kvp => (TN)kvp.Value));
    }

    public virtual Task HandleDeploy(string projectCode, Dictionary<string, TN> nodes)
    {
        return Task.CompletedTask;
    }

    public Task Execute(in object nodeData, ref SessionData session, ref string output)
    {
        return Execute((TN)nodeData, ref session, ref output);
    }

    public abstract Task Execute(in TN nodeData, ref SessionData session, ref string output);
}