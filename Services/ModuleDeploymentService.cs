using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WilsonEvoCoreLibrary.Core.Utility;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services;
public class DeploymentResult
{
    public bool IsSuccessful { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public DeploymentResult()
    {
        IsSuccessful = true; // Assume success unless an error is added
    }

    public void AddError(string error)
    {
        IsSuccessful = false;
        Errors.Add(error);
    }
}
public sealed class ModuleDeploymentService(ModuleConfiguration _config, IServiceProvider _serviceProvider)
{
    public async Task<DeploymentResult> DeployModulesAsync(UpdateModuleResponse updateRequest)
    {
        if (updateRequest == null)
            throw new ArgumentNullException(nameof(updateRequest));

        var result = new DeploymentResult();

        try
        {
            foreach (var environment in updateRequest.Environments)
            {
                var taskGroups = environment.Tasks.GroupBy(task => new { task.ModelType, task.ChannelType });

                foreach (var taskGroup in taskGroups)
                {
                    var handlerPair = _config.DeploymentHandlers.FirstOrDefault(h => h.Key.Name.Equals(taskGroup.Key.ModelType));

                    if (handlerPair.Equals(default(KeyValuePair<Type, Type>)))
                    {
                        result.AddError($"Handler not found for ModelType '{taskGroup.Key.ModelType}'.");
                        continue;
                    }

                    var deploymentService = _serviceProvider.GetService(handlerPair.Value) as IEnvironmentDeploy;
                    if (deploymentService == null)
                    {
                        result.AddError($"Deployment service not found or invalid for ModelType '{taskGroup.Key.ModelType}'.");
                        continue;
                    }

                    var tasksById = new Dictionary<string, ITask>();
                    foreach (var taskItem in taskGroup)
                    {
                        var taskData = await BinarySerialization.DeserializeWithType(taskItem.data, handlerPair.Key);
                        if (taskData == null)
                        {
                            result.AddError($"Failed to deserialize task data for NodeId '{taskItem.NodeId}'.");
                            continue;
                        }
                        if (taskData is ITask task)
                        {
                            tasksById.Add(taskItem.NodeId, task);
                        }
                        else
                        {
                            result.AddError($"Deserialized data is not of type ITask for NodeId '{taskItem.NodeId}'.");
                        }
                    }

                    try
                    {
                        await deploymentService.HandleDeployInternal(environment.ShortUrl, tasksById);
                    }
                    catch (Exception ex)
                    {
                        result.AddError($"Error during deployment to environment '{environment.ShortUrl}': {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.AddError($"An unexpected exception occurred: {ex.Message}");
        }

        return result;
    }
}