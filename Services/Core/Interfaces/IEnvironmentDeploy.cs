using System.Collections.Generic;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

public interface IEnvironmentDeploy<T> : IEnvironmentDeploy where T : ITask
{
    Task HandleDeploy(string projectCode, Dictionary<string, T> nodes);
}

public interface IEnvironmentDeploy
{
    Task HandleDeployInternal(string projectCode, Dictionary<string, ITask> nodes);
}