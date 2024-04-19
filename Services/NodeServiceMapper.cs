using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        try
        {
            if (!GetServiceType(request.Type, request.SessionData.CommunicationChannel, out var serviceType))
            {
                session.SessionExceptionDetails = $"Missing service for: {request.Type} and {request.SessionData.CommunicationChannel}"; //too see
                session.ProcessCurrentOutput = "error";
                session.SessionEncounteredError = true;
            }
            else
            {
                var service = GetService(serviceType);
                var node = await BinarySerialization.DeserializeWithType(request.NodeData, serviceType.GenericTypeArguments[0]);
                // var node = await ReadSessionData(request, );
                if (node == null)
                {
                    session.SessionEncounteredError = true;
                    session.SessionExceptionDetails = $"No data in {request.Type}.";
                    session.ProcessCurrentOutput = "error";
                }
                else
                {
                    if (service is IExecutionService syncService)
                    {
                        await syncService.Execute(in node, ref session, ref output);
                    }
                    else if (service is IAsyncExecutionService asyncService && !session.IsAwaitingCallback)
                    {
                        session.Await();
                        await asyncService.Execute(in node, ref session, ref output);
                    }
                    else if (service is IAsyncExecutionService asyncServiceCallback && session.IsAwaitingCallback)
                    {
                        session.IsAwaitingCallback = false;
                        await asyncServiceCallback.ExecuteCallback(in node, ref session, ref output);
                    }
                    else
                    {
                        //this is shit where i go?          
                        session.IsAwaitingCallback = false;
                        session.SessionEncounteredError = true;
                        session.ProcessCurrentOutput = "error";
                        session.SessionExceptionDetails = "Module service not found.";
                    }

                    session.ProcessCurrentOutput = output;
                }

                
            }
        }
        catch (Exception e)
        {
            session.SessionEncounteredError = true;
            session.SessionExceptionDetails = e.Message;
        }


        response.SessionData = session;
        return response;
    }
}