using System;
using System.Threading.Tasks;
using WilsonEvoCoreLibrary.Core.Errors;
using WilsonEvoCoreLibrary.Core.Models;
using WilsonEvoCoreLibrary.Core.Utility;
using WilsonEvoModuleLibrary.Entities;
using ServiceProvider = WilsonEvoModuleLibrary.Utility.ServiceProvider;

namespace WilsonEvoModuleLibrary.Services;

public sealed class NodeServiceExecutor(ModuleConfiguration config, ServiceProvider serviceRegistry)
{
    public async Task<ServiceResponse> HandleServiceRequest(ServiceRequest request)
    {
        var session = request.SessionData;
        await ExecuteNode(request.Type, session, request.NodeData, request.ModuleConfiguration);

        var response = new ServiceResponse { Session = session, ExecutionTime = 0, ModuleUsage = 0 };
        return response;
    }

    private async Task ExecuteNode(string nodeType, ProcessSession session, byte[] rawNodeData, byte[] rawModuleConfiguration)
    {
        session.NodeOutput = "ok";
        try
        {
            var (taskType, executor, executorKind) = serviceRegistry.GetExecutor(nodeType, session.ChannelName);
            if (false)
            {
                session.Error(WorkflowError.ModuleMissingService, $"Missing service for: {nodeType} and {session.ChannelName}");
                session.NodeOutput = "error";
            }

            var nodeData = await BinarySerialization.DeserializeWithType(rawNodeData, taskType);
            object? moduleConfiguration = null;
            if (config.ModuleConfigurationType != null)
            {
                if (rawModuleConfiguration is null)
                {
                    moduleConfiguration = Activator.CreateInstance(config.ModuleConfigurationType);
                }
                else
                {
                    moduleConfiguration = await BinarySerialization.DeserializeWithType(rawModuleConfiguration, config.ModuleConfigurationType);
                }
            }

            if (nodeData == null)
            {
                session.Error(WorkflowError.ModuleMissingNodeData, $"No data in {nodeType}.");
                return;
            }
            session.Init();
            switch (executorKind)
            {
                case ExecutorType.Synchronous:
                    await ExecutorInvoker.InvokeExecuteAsync(executor, nodeData, session, moduleConfiguration);
                    break;
                case ExecutorType.Asynchronous when !session.IsAwaitingCallback:
                    session.IsAwaitingCallback = true;
                    session.ContinueExecution = false;
                    await ExecutorInvoker.InvokeExecuteAsync(executor, nodeData, session, moduleConfiguration);
                    break;
                case ExecutorType.Asynchronous when session.IsAwaitingCallback:
                    session.IsAwaitingCallback = false;
                    session.ContinueExecution = true;
                    await ExecutorInvoker.InvokeExecuteCallbackAsync(executor, nodeData, session, moduleConfiguration);
                    break;
                default:
                    session.IsAwaitingCallback = false;
                    session.Error(WorkflowError.ModuleMissingServiceInterface, $"Module service not found for {nodeType}");
                    break;
            }
            session.VerifyAndSaveChanges();
        }
        catch (Exception e)
        {
            session.IsAwaitingCallback = false;
            session.Error(WorkflowError.ModuleGenericException, e.Message);
        }
    }
}