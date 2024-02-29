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
    private readonly ServiceMappings _map;
    private readonly IServiceProvider _servicesProvider;

    public NodeServiceMapper(IServiceProvider service, ServiceMappings map, ModelsConfiguration config)
    {                                                                        
        _servicesProvider = service;
        _map = map;
    }

    private object? GetService(Type? type)
    {
        if (type != null)
        {
            using var scope = _servicesProvider.CreateScope();
            var service = scope.ServiceProvider.GetService(type);
            if (service != null)
            {
                return service;
            }

            return null; 
        }
        return null;
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

                if (serviceType == null)
                {
                    throw new Exception($"Invalid serviceType on the module update  for: {group.Key.ModelType} and {group.Key.ChannelType}."); 
                }

                var service = GetService(serviceType);
                if (service != null)
                {
                    var listNodes = new Dictionary<string, BaseTask>();
                    foreach (var task in group)
                    {
                        if (task.data is null)
                            continue;
                        var data = await BinarySerialization.DeserializeWithType(task.data, serviceType.GenericTypeArguments[0]);
                        if (data is BaseTask baseTask)
                        {
                            listNodes.Add(task.NodeId, baseTask);
                        }
                        else
                        {
                            throw new Exception("Invalid data type");
                        }
                    }

                    if (service is IEnvironmentDeploy serviceI)
                    {
                        await serviceI.HandleDeployInternal(updateRequest.projectCode, listNodes);
                    }
                }
                else
                {
                    throw new Exception($"No service found  for: {group.Key.ModelType} and {group.Key.ChannelType} on the module update.");
                }
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return "ok";
    }


    public async Task ExecuteService(SessionData session, string type, string channelType, byte[] data)
    {
        var output = "ok";
        try
        {
            if (!GetServiceType(type, channelType, out var serviceType))
            {
                session.SetError($"Missing service for: {type} and {channelType}");
                return;
            }
            if (serviceType == null)
            {
                session.SetError($"Service is null for: {type} and {channelType}");
                return;
            }

            var service = GetService(serviceType);
            var nodeData = await BinarySerialization.DeserializeWithType(data, serviceType.GenericTypeArguments[0]);
            if (nodeData == null)
            {
                session.SetError($"No data present in node for: {session.SessionId} Service: {type} and Channel: {channelType}.");
                return;
            }

            switch (service)
            {
                case IExecutionService syncService:
                    await syncService.Execute(in nodeData, ref session, ref output);
                    session.CurrentOutput = output;
                    break;
                case IAsyncExecutionService asyncService when !session.WaitingCallback:
                    await asyncService.Execute(in nodeData, ref session, ref output);
                    session.CurrentOutput = output;
                    session.WaitingCallback = true;
                    session.ContinueExecution = false;
                    break;
                case IAsyncExecutionService asyncServiceCallback when session.WaitingCallback:
                    await asyncServiceCallback.ExecuteCallback(in nodeData, ref session, ref output);
                    session.CurrentOutput = output;
                    session.WaitingCallback = false;
                    break;
                default:
                    session.SetError($"Module service not found session id {session.SessionId} for type {type} channel {channelType}.");
                    break;
            }
        }
        catch (Exception e)
        {
            session.SetError(e.Message);
        }
    }
}