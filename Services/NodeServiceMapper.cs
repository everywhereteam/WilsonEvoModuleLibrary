using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public object? GetService(Type? type)
    {
        try
        {     
            if (type != null)
            {

                using var scope = _servicesProvider.CreateScope();
                var service = scope.ServiceProvider.GetService(type);
                if (service != null)
                {
                    return service;
                }
                else
                {
                    return null;//Result.Fail($"The service {type} can't be instantiated.");
                }
            }
            else
            {
                return null;// Result.Fail("The service type is null.");
            }
        }
        catch (Exception ex)
        {
            return null;//Result.Fail($"Error getting the service module for: {type} and {channelType} \n {ex.Message}");
        }
    }

    private bool GetServiceType(string type, string channelType, out Type? serviceType)
    {
        if (!_map.ServiceMap.TryGetValue(new MapPath(type, channelType), out serviceType))
        {
            if (!_map.ServiceMap.TryGetValue(new MapPath(type, string.Empty), out serviceType))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<string> UpdateService(UpdateRequest updateRequest)
    {
        try
        {
            foreach (var group in updateRequest.Tasks.GroupBy(x => new { x.ModelType, x.ChannelType }))
            {
                if (!GetServiceType(group.Key.ModelType, group.Key.ChannelType, out var serviceType))
                {
                    return $"Missing service for: {group.Key.ModelType} and {group.Key.ChannelType}";
                }
                var service = GetService(serviceType);
                if (service != null)
                {
                  
                    var listNodes = new Dictionary<string,BaseTask>();
                    foreach (var task in group)
                    {
                        var data = await BinarySerialization.DeserializeWithType(task.data, serviceType.GenericTypeArguments[0]);
                        listNodes.Add(task.NodeId,(BaseTask)data);
                    }
                    if (service is IEnvironmentDeploy serviceI)
                    {
                        await serviceI.HandleDeployInternal(updateRequest.projectCode,listNodes);
                       
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

        if (!GetServiceType(request.Type, request.SessionData.ChannelType, out var serviceType))
        {
            session.Exception = $"Missing service for: {request.Type} and {request.SessionData.ChannelType}";   //too see
            session.CurrentOutput = "error";
            session.IsFaulted = true;
        }
        else
        {
            var service = GetService(serviceType);
            var node = await BinarySerialization.DeserializeWithType(request.NodeData, serviceType.GenericTypeArguments[0]);
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