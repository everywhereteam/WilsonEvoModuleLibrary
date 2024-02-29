using System.Collections.Generic;
using System.Threading.Tasks;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Services.Core.Interfaces;

public interface IEnvironmentDeploy<T> : IEnvironmentDeploy where T : BaseTask
{
    Task HandleDeploy(string projectCode, Dictionary<string, T> nodes);
}

public interface IEnvironmentDeploy
{
    Task HandleDeployInternal(string projectCode, Dictionary<string, BaseTask> nodes);
}