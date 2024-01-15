using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services.Core;

public abstract class NodeServices<TN, TC> : IExecutionService, IEnvironmentDeploy<TN>, INodeServices<TN, TC>
    where TN : BaseTask where TC : class
{
    public Task Execute(in object nodeData, ref SessionData data, ref string output)
    {
        return Execute((TN)nodeData, ref data, ref output);
    }

    public abstract Task Execute(in TN nodeData, ref SessionData data, ref string output);

    public Task HandleDeployInternal(string projectCode, Dictionary<string, BaseTask> nodes)
    {
        return HandleDeploy(projectCode, nodes.ToDictionary(kvp => kvp.Key, kvp => (TN)kvp.Value));
    }

    public virtual Task HandleDeploy(string projectCode, Dictionary<string, TN> nodes)
    {
        return Task.CompletedTask;
    }
}