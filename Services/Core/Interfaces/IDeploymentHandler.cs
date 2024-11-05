using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

public interface IDeploymentHandler<TTask> 
{
    public Task HandleDeployInternal(uint envId, Dictionary<string, object> nodes)
    {
        var typedNodes = nodes.ToDictionary(kvp => kvp.Key, kvp => (TTask)kvp.Value);
        return HandleDeploy(envId, typedNodes);
    }

    Task HandleDeploy(uint envId, Dictionary<string, TTask> nodes);
}