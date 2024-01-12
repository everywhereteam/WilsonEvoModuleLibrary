using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;
using WilsonEvoModuleLibrary.Utility;

namespace WilsonEvoModuleLibrary.Services;

public sealed class NodeServiceMapper
{
    private readonly ModelsConfiguration _config;
    private readonly ServiceMappings _map;
    private readonly IServiceProvider _servicesProvider;

    public NodeServiceMapper(IServiceProvider service, ServiceMappings map, ModelsConfiguration config)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        _servicesProvider = service;
        _map = map;
        _config = config;
    }

    public Result<object> GetService(string type, string channelType)
    {
        try
        {
            if (!_map.ServiceMap.TryGetValue(new MapPath(type, channelType), out var serviceType))
            {
                if (!_map.ServiceMap.TryGetValue(new MapPath(type, string.Empty), out serviceType))
                {
                    return Result.Fail($"Missing service for: {type} and {channelType}");
                }
            }

            if (serviceType != null)
            {

                using var scope = _servicesProvider.CreateScope();
                var service = scope.ServiceProvider.GetService(serviceType);
                if (service != null)
                {
                    return Result.Ok(service);
                }
                else
                {
                    return Result.Fail($"The service {type} can't be instantiated.");
                }
            }
            else
            {
                return Result.Fail("The service type is null.");
            }
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error getting the service module for: {type} and {channelType} \n {ex.Message}");
        }
    }

    public async Task<string> UpdateService(UpdateRequest updateRequest)
    {
        try
        {
            foreach (var task in updateRequest.Tasks)
            {
                var serviceResult = GetService(task.ModelType, task.ChannelType);
                if (serviceResult.IsSuccess)
                {
                    var service = serviceResult.Value;
                    List<BaseTask> nodesToUpdate = new();
                    foreach (var taskData in updateRequest.Tasks)
                    {
                        var node = await BinarySerialization.DeserializeWithType(taskData.data, service.GetType().GenericTypeArguments[0]);
                        if (node != null)
                        {
                            nodesToUpdate.Add((BaseTask)node);
                        }
                    }

                    if (service is IEnvironmentDeploy serviceI)
                    {
                        await serviceI.HandleDeploy(nodesToUpdate);
                    }
                }
            }
        }
        catch (Exception ex)
        {

            return ex.Message;
        }

        return "ok";
    }


    public async Task<ServiceResponse> ExecuteService(ServiceRequest request)
    {
        var session = request.SessionData;

        var response = new ServiceResponse();
        var output = "ok";
        var serviceResult = GetService(request.Type, request.SessionData.ChannelType);

        if (serviceResult.IsFailed)
        {
            session.Exception = serviceResult.ToString();   //too see
            session.CurrentOutput = "error";
            session.IsFaulted = true;
        }
        else
        {
            var service = serviceResult.Value;
            var node = await BinarySerialization.DeserializeWithType(request.NodeData, service.GetType().GenericTypeArguments[0]);
           // var node = await ReadSessionData(request, );

            if (service is IExecutionService syncService)
            {
                await syncService.Execute(in node, ref session, ref output);
            }
            else if (service is IAsyncExecutionService asyncService && !session.WaitingCallback)
            {
                await asyncService.Execute(in node, ref session, ref output);
                session.WaitingCallback = true;
                session.ContinueExecution = false;
            }
            else if (service is IAsyncExecutionService asyncServiceCallback && session.WaitingCallback)
            {
                await asyncServiceCallback.ExecuteCallback(in node, ref session, ref output);
                session.WaitingCallback = false;
            }
            else
            {
                //this is shit where i go?
                session.ContinueExecution = false;
                session.WaitingCallback = false;
                session.IsFaulted = true;
                session.Exception = "Module service not found.";
            }
            session.CurrentOutput = output;
        }


        response.SessionData = session;
        return response;
    } 
}